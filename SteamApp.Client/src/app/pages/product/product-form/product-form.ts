import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ReactiveFormsModule, FormBuilder, Validators } from '@angular/forms';
import { ActivatedRoute, Router } from '@angular/router';
import { GameService, ProductService } from '../../../services';
import { CreateProduct, Game, UpdateProduct } from '../../../models';



@Component({
  selector: 'steam-product-form',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule],
  templateUrl: './product-form.html',
  styleUrl: './product-form.scss',
})
export class ProductForm implements OnInit {
  isEditMode = false;
  productId?: number;

  form = this.fb.nonNullable.group({
    gameId: [0],
    name: [''],
    rating: [null],
    isActive: [true],
  });

  constructor(
    private fb: FormBuilder,
    private route: ActivatedRoute,
    private router: Router,
    private productService: ProductService,
    private gameService: GameService,
  ) {}

  ngOnInit(): void {
    const idParam = this.route.snapshot.paramMap.get('id');

    this.loadGames();

    if (idParam) {
      this.isEditMode = true;
      this.productId = Number(idParam);
      this.loadProduct(this.productId);
    }
  }


  private loadProduct(id: number): void {
    this.productService.getById(id).subscribe((product) => {
      this.form.patchValue({
        gameId: product.gameId,
        name: product.name ?? '',
        isActive: product.isActive ?? false,
      });

      this.form.controls.gameId.disable();
    });
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
    if (this.form.invalid) {
      return;
    }

    if (this.isEditMode && this.productId) {
      const update: UpdateProduct = {
        name: this.form.controls.name.value,
        isActive: this.form.controls.isActive.value,
        rating: this.form.controls.rating.value,
      };

      this.productService.update(this.productId, update).subscribe(() => {
        this.router.navigate(['/products']);
      });
    } else {
      const create: CreateProduct = this.form.getRawValue();

      this.productService.create(create).subscribe(() => {
        this.router.navigate(['/products']);
      });
    }
  }

  cancel(): void {
    this.router.navigate(['/products']);
  }
}
