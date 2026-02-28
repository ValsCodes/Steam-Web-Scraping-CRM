import { ChangeDetectorRef, Component, OnDestroy, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import {
  ReactiveFormsModule,
  FormBuilder,
  Validators,
  FormControl,
} from '@angular/forms';
import { ActivatedRoute, Router } from '@angular/router';
import { CreateGameUrlProduct, Game, GameUrl, Product } from '../../../models';
import {
  GameService,
  GameUrlProductService,
  GameUrlService,
  ProductService,
} from '../../../services';
import { Subject, takeUntil } from 'rxjs';
import { Title } from '@angular/platform-browser';

@Component({
  selector: 'steam-game-url-product-form',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule],
  templateUrl: './game-url-product-form.html',
  styleUrl: './game-url-product-form.scss',
})
export class GameUrlProductForm implements OnInit, OnDestroy {
  isEditMode = false;

  originalProductId?: number;
  originalGameUrlId?: number;

  form = this.fb.nonNullable.group({
    productId: [0, Validators.required],
    gameUrlId: [0, Validators.required],
  });

  private readonly destroy$ = new Subject<void>();

  constructor(
    private fb: FormBuilder,
    private route: ActivatedRoute,
    private router: Router,
    private service: GameUrlProductService,
    private gameUrlService: GameUrlService,
    private productService: ProductService,
    private gameService: GameService,
    private readonly cdr: ChangeDetectorRef,
  ) {
  }

  ngOnDestroy(): void {
    this.destroy$.next();
    this.destroy$.complete();
  }

  games: Game[] = [];
  gameIdControl = new FormControl<number | null>(null);

  gameUrlsAll: GameUrl[] = [];
  gameUrlsFiltered: GameUrl[] = [];
  gameUrlIdControl = new FormControl<number | null>(null);

  productsAll: Product[] = [];
  productsFiltered: Product[] = [];
  productIdControl = new FormControl<number | null>(null);

  ngOnInit(): void {
    this.loadGames();
    this.loadGameUrls();
    this.loadProducts();

    const productId = this.route.snapshot.paramMap.get('productId');
    const gameUrlId = this.route.snapshot.paramMap.get('gameUrlId');

    if (productId && gameUrlId) {
      this.isEditMode = true;

      this.originalProductId = Number(productId);
      this.originalGameUrlId = Number(gameUrlId);

      this.form.patchValue({
        productId: this.originalProductId,
        gameUrlId: this.originalGameUrlId,
      });

      this.form.controls.productId.disable();
      this.form.controls.gameUrlId.disable();
    }

    this.bindGameSelection();
  }
  private loadGames(): void {
    this.gameService
      .getAll()
      .pipe(takeUntil(this.destroy$))
      .subscribe((games) => {
        this.games = games;
        this.cdr.markForCheck();
      });
  }

  private loadGameUrls(): void {
    this.gameUrlService
      .getAll()
      .pipe(takeUntil(this.destroy$))
      .subscribe((urls) => {
        this.gameUrlsAll = urls;
        this.applyGameFilter(this.gameIdControl.value);
      });
  }

  private loadProducts(): void {
    this.productService
      .getAll()
      .pipe(takeUntil(this.destroy$))
      .subscribe((products) => {
        this.productsAll = products;
        this.applyGameFilter(this.gameIdControl.value);
      });
  }

  private bindGameSelection(): void {
    this.gameIdControl.valueChanges.subscribe((gameId) => {
      this.applyGameFilter(gameId);
    });
  }

  private applyGameFilter(gameId: number | null): void {
    if (gameId === null) {
      this.gameUrlsFiltered = [];
      this.gameUrlIdControl.reset();

      this.productsFiltered = [];
      this.productIdControl.reset();

      this.cdr.markForCheck();
      return;
    }

    this.gameUrlsFiltered = this.gameUrlsAll.filter(
      (url) => url.gameId === gameId,
    );

    this.productsFiltered = this.productsAll.filter(
      (product) => product.gameId === gameId,
    );

    this.gameUrlIdControl.reset();
    this.productIdControl.reset();
    this.cdr.markForCheck();
  }

  onSubmit(): void {
    if (this.form.invalid) {
      return;
    }

    const payload: CreateGameUrlProduct = this.form.getRawValue();

    if (
      this.isEditMode &&
      this.originalProductId !== undefined &&
      this.originalGameUrlId !== undefined
    ) {
      this.service
        .delete(this.originalProductId, this.originalGameUrlId)
        .subscribe(() => {
          this.service.create(payload).subscribe(() => {
             this.router.navigate(['/game-url-products']);
          });
        });

      return;
    }

    this.service.create(payload).subscribe(() => {
      this.router.navigate(['/game-url-products']);
    });
  }

  cancel(): void {
    this.router.navigate(['/game-url-products']);
  }
}
