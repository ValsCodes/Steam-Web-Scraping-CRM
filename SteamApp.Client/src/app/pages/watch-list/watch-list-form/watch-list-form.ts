import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ReactiveFormsModule, FormBuilder, Validators } from '@angular/forms';
import { ActivatedRoute, Router } from '@angular/router';
import { GameService, GameUrlService, ProductService, WatchListService } from '../../../services';
import { CreateWatchList, Game, GameUrl, Product, UpdateWatchList } from '../../../models';

@Component({
  selector: 'steam-watch-list-form',
  standalone: true,
  imports: [
    CommonModule,
    ReactiveFormsModule
  ],
  templateUrl: './watch-list-form.html',
  styleUrl: './watch-list-form.scss'
})
export class WatchListForm implements OnInit {
  isEditMode = false;
  watchListId?: number;

  form = this.fb.nonNullable.group({
    gameId: [0],
    gameUrlId: [null as number | null],
    rating: [null as number | null],
    batchNumber: [null as number | null],
    name: [''],
    productId: [null as number | null],
    releaseDate: ['', Validators.required],
    description: [''],
    isActive: [true]
  });

  constructor(
    private fb: FormBuilder,
    private route: ActivatedRoute,
    private router: Router,
    private watchListService: WatchListService,
    private gameService: GameService,
    private gameUrlService: GameUrlService,
    private productService: ProductService,
  ) {}

  ngOnInit(): void {
    const idParam = this.route.snapshot.paramMap.get('id');
    this.loadGames();
    this.loadGameUrls();
    this.loadProducts();

    if (idParam) {
      this.isEditMode = true;
      this.watchListId = Number(idParam);
      this.loadWatchList(this.watchListId);
    }
  }

  private loadWatchList(id: number): void {
    this.watchListService.getById(id).subscribe(item => {
      this.form.patchValue({
        gameId: item.gameId ?? 0,
        gameUrlId: item.gameUrlId ?? null,
        productId: item.productId ?? null,
        rating: item.rating ?? null,
        batchNumber: item.batchNumber ?? null,
        name: item.name ?? '',
        releaseDate: item.releaseDate ?? new Date().toISOString().substring(0, 10),
        description: item.description ?? '',
        isActive: item.isActive
      });
    });
  }

  games: Game[] = [];
  gameNameById = new Map<number, string>();

  private loadGames(): void
{
    this.gameService.getAll().subscribe({
        next: games =>
        {
            this.games = games;

            this.gameNameById.clear();
            for (const game of games) {
                this.gameNameById.set(game.id, game.name);
            }
        }
    });
}

  gameUrls: GameUrl[] = [];
  gameUrlNameById = new Map<number, string>();

  private loadGameUrls(): void
{
    this.gameUrlService.getAll().subscribe({
        next: gameUrls =>
        {
            this.gameUrls = gameUrls;

            this.gameUrlNameById.clear();
            for (const gameUrl of gameUrls) {
                this.gameUrlNameById.set(gameUrl.id, gameUrl.name ?? '-');
            }
        }
    });
}

  products: Product[] = [];
  productNameById = new Map<number, string>();

  private loadProducts(): void {
    this.productService.getAll().subscribe({
        next: products => {
            this.products = products;

            this.productNameById.clear();
            for (const product of products) {
                this.productNameById.set(product.id, product.name ?? '-');}
        }
    });
  }

  onSubmit(): void {
    if (this.form.invalid) {
      return;
    }

    if (this.isEditMode && this.watchListId) {
      const update: UpdateWatchList = this.form.getRawValue();

      this.watchListService.update(this.watchListId, update).subscribe(() => {
        this.router.navigate(['/watch-list']);
      });
    } else {
      const create: CreateWatchList = this.form.getRawValue();

      this.watchListService.create(create).subscribe(() => {
        this.router.navigate(['/watch-list']);
      });
    }
  }

  cancel(): void {
    this.router.navigate(['/watch-list']);
  }
}
