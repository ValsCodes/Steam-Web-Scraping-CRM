import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ReactiveFormsModule, FormBuilder, Validators } from '@angular/forms';
import { ActivatedRoute, Router } from '@angular/router';
import { finalize, Observable } from 'rxjs';
import { GameService, WishListService } from '../../../services';
import { CreateWishList, Game, UpdateWishList } from '../../../models';


@Component({
  selector: 'steam-wish-list-form',
  standalone: true,
  imports: [
    CommonModule,
    ReactiveFormsModule
  ],
  templateUrl: './wish-list-form.html',
  styleUrl: './wish-list-form.scss'
})
export class WishListForm implements OnInit {
  isEditMode = false;
  wishListId?: number;
  isSubmitting = false;

  form = this.fb.nonNullable.group({
    gameId: [0, Validators.required],
    price: [null as number | null],
    isActive: [true],
    name: [null as string | null],
  });

  constructor(
    private fb: FormBuilder,
    private route: ActivatedRoute,
    private router: Router,
    private wishListService: WishListService,
    private gameService: GameService
  ) {}

  ngOnInit(): void {
    const idParam = this.route.snapshot.paramMap.get('id');
    this.loadGames();

    if (idParam) {
      this.isEditMode = true;
      this.wishListId = Number(idParam);
      this.loadWishList(this.wishListId);
    }
  }

  private loadWishList(id: number): void {
    this.wishListService.getById(id).subscribe(item => {
      this.form.patchValue({
        gameId: item.gameId,
        price: item.price,
        isActive: item.isActive,
        name: item.name,  
      });
    });

    this.form.controls.gameId.disable();
  }

    games: Game[] = [];
    gameNameById = new Map<number, string>();
  
    private loadGames(): void {
      this.gameService.getAll().subscribe({
        next: (games) => {
          this.games = games;
  
          this.gameNameById.clear();
          for (const game of games) {
            this.gameNameById.set(game.id, game.name);
          }
        },
      });
    }

  onSubmit(): void {
    if (this.form.invalid || this.isSubmitting) {
      return;
    }

    this.isSubmitting = true;

    const request$: Observable<unknown> = this.isEditMode && this.wishListId
      ? this.wishListService.update(this.wishListId, {
          price: this.form.controls.price.value,
          isActive: this.form.controls.isActive.value,
          name: this.form.controls.name.value,
        } as UpdateWishList)
      : this.wishListService.create(this.form.getRawValue() as CreateWishList);

    request$
      .pipe(finalize(() => {
        this.isSubmitting = false;
      }))
      .subscribe(() => {
        this.router.navigate(['/wishlist']);
      });
  }

  cancel(): void {
    this.router.navigate(['/wishlist']);
  }
}
