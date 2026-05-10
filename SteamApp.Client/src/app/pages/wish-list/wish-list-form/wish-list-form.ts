import { Component, DestroyRef, OnInit, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ReactiveFormsModule, FormBuilder, Validators } from '@angular/forms';
import { ActivatedRoute, Router } from '@angular/router';
import { finalize, map, Observable } from 'rxjs';
import { takeUntilDestroyed, toSignal } from '@angular/core/rxjs-interop';
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
  private readonly destroyRef = inject(DestroyRef);

  isEditMode = false;
  wishListId?: number;
  isSubmitting = false;

  form = this.fb.nonNullable.group({
    gameId: [null as number | null, [Validators.required, Validators.min(1)]],
    price: [null as number | null],
    isActive: [true],
    name: ['', Validators.required],
  });

  readonly games = toSignal(
    this.gameService.getAll().pipe(
      map(games => games.filter(game => game.isActive)),
    ),
    { initialValue: [] as Game[] },
  );

  constructor(
    private fb: FormBuilder,
    private route: ActivatedRoute,
    private router: Router,
    private wishListService: WishListService,
    private gameService: GameService
  ) {}

  ngOnInit(): void {
    const idParam = this.route.snapshot.paramMap.get('id');

    if (idParam) {
      this.isEditMode = true;
      this.wishListId = Number(idParam);
      this.form.controls.gameId.disable({ emitEvent: false });
      this.loadWishList(this.wishListId);
    }
  }

  private loadWishList(id: number): void {
    this.wishListService
      .getById(id)
      .pipe(takeUntilDestroyed(this.destroyRef))
      .subscribe(item => {
        this.form.patchValue({
          gameId: item.gameId,
          price: item.price,
          isActive: item.isActive,
          name: item.name ?? '',
        });
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
      .pipe(
        takeUntilDestroyed(this.destroyRef),
        finalize(() => {
          this.isSubmitting = false;
        }),
      )
      .subscribe(() => {
        this.router.navigate(['/wishlist']);
      });
  }

  cancel(): void {
    this.router.navigate(['/wishlist']);
  }
}
