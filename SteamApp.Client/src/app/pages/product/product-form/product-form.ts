import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ReactiveFormsModule, FormBuilder, Validators } from '@angular/forms';
import { ActivatedRoute, Router } from '@angular/router';

import { CreateProduct, UpdateProduct } from '../../../models/product.model';
import { ProductService } from '../../../services/product/product.service';
import { GameUrlService } from '../../../services/game-url/game-url.service';
import { GameService } from '../../../services/game/game.service';
import { GameUrl } from '../../../models/game-url.model';
import { Game } from '../../../models';

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
    gameId: [null],
    gameUrlId: [0, Validators.required],
    name: [''],
  });

  constructor(
    private fb: FormBuilder,
    private route: ActivatedRoute,
    private router: Router,
    private productService: ProductService,
    private gameUrlService: GameUrlService,
    private gameService: GameService,
  ) {}

  ngOnInit(): void {
    const idParam = this.route.snapshot.paramMap.get('id');

    this.loadGames();
    this.loadGameUrls();

    if (idParam) {
      this.isEditMode = true;
      this.productId = Number(idParam);
      this.loadProduct(this.productId);
    }

    this.form.controls.gameId.valueChanges.subscribe((value) => {
      const gameId = Number(value);

      if (Number.isNaN(gameId)) {
        this.filteredGameUrls = [];
        this.form.controls.gameUrlId.reset();
        return;
      }

      this.filteredGameUrls = this.gameUrls.filter((x) => x.gameId === gameId);
      this.form.controls.gameUrlId.reset();
    });
  }

  filteredGameUrls: GameUrl[] = [];

  private loadProduct(id: number): void {
    this.productService.getById(id).subscribe((product) => {
      this.form.patchValue({
        gameUrlId: product.gameUrlId,
        name: product.name ?? '',
      });

      // GameUrlId is immutable after creation
      this.form.controls.gameUrlId.disable();
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

  gameUrls: GameUrl[] = [];
  gameUrlNameById = new Map<number, string>();

  private loadGameUrls(): void {
    this.gameUrlService.getAll().subscribe({
      next: (urls) => {
        this.gameUrls = urls;
        this.filteredGameUrls = [];
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
