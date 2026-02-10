import { AfterViewInit, Component, OnInit, ViewChild } from '@angular/core';
import { CommonModule } from '@angular/common';
import { Router } from '@angular/router';
import { FormControl } from '@angular/forms';
import { BehaviorSubject, startWith, tap } from 'rxjs';

import { MatTableModule, MatTableDataSource } from '@angular/material/table';
import { MatPaginator, MatPaginatorModule } from '@angular/material/paginator';
import { MatSort, MatSortModule } from '@angular/material/sort';

import { Game, GameUrl, Product, GameUrlProduct } from '../../../models';
import {
  GameService,
  GameUrlService,
  ProductService,
  GameUrlProductService,
} from '../../../services';
import { ComboBoxComponent } from '../../../components/filter-components/combo-box-filter.component';

@Component({
  selector: 'app-game-url-products',
  standalone: true,
  templateUrl: './game-url-products-view.html',
  imports: [
    CommonModule,
    MatTableModule,
    MatPaginatorModule,
    MatSortModule,
    ComboBoxComponent,
  ],
})
export class GameUrlProductsView implements AfterViewInit, OnInit {
  displayedColumns: string[] = [
    'productName',
    'gameUrlName',
    'fullUrl',
    'isBatchUrl',
    'actions',
  ];

  dataSource = new MatTableDataSource<GameUrlProduct>([]);

  gameUrlProducts: GameUrlProduct[] = [];
  filteredGameUrlProducts: GameUrlProduct[] = [];

  readonly games$ = new BehaviorSubject<readonly Game[]>([]);
  readonly gameUrlsFiltered$ = new BehaviorSubject<readonly GameUrl[]>([]);
  readonly productsFiltered$ = new BehaviorSubject<readonly Product[]>([]);

  readonly gameIdControl = new FormControl<number | null>(null);
  readonly gameUrlIdControl = new FormControl<number | null>(null);
  readonly productIdControl = new FormControl<number | null>(null);

  readonly bindGameToExternal$ = this.gameIdControl.valueChanges.pipe(
    startWith(this.gameIdControl.value),
    tap(gameId => this.applyGameFilter(gameId))
  );

  readonly bindGameUrlToExternal$ = this.gameUrlIdControl.valueChanges.pipe(
    startWith(this.gameUrlIdControl.value),
    tap(() => this.loadFilteredGameUrlProducts())
  );

  readonly bindProductToExternal$ = this.productIdControl.valueChanges.pipe(
    startWith(this.productIdControl.value),
    tap(() => this.loadFilteredGameUrlProducts())
  );

  private games: Game[] = [];
  private gameUrlsAll: GameUrl[] = [];
  private productsAll: Product[] = [];

  @ViewChild(MatPaginator) paginator!: MatPaginator;
  @ViewChild(MatSort) sort!: MatSort;

  constructor(
    private readonly gameUrlProductService: GameUrlProductService,
    private readonly gameService: GameService,
    private readonly gameUrlService: GameUrlService,
    private readonly productService: ProductService,
    private readonly router: Router,
  ) {}

  ngOnInit(): void {
    this.loadGames();
    this.loadGameUrls();
    this.loadProducts();
    this.fetchGameUrlProducts();
  }

  ngAfterViewInit(): void {
    this.dataSource.paginator = this.paginator;
    this.dataSource.sort = this.sort;
  }

  private loadGames(): void {
    this.gameService.getAll().subscribe(games => {
      this.games = games;
      this.games$.next(games);
    });
  }

  private loadGameUrls(): void {
    this.gameUrlService.getAll().subscribe(urls => {
      this.gameUrlsAll = urls;
      this.applyGameFilter(this.gameIdControl.value);
    });
  }

  private loadProducts(): void {
    this.productService.getAll().subscribe(products => {
      this.productsAll = products;
      this.applyGameFilter(this.gameIdControl.value);
    });
  }

  fetchGameUrlProducts(): void {
    this.gameUrlProductService.getAll().subscribe(items => {
      this.gameUrlProducts = items;
      this.filteredGameUrlProducts = items;
      this.dataSource.data = items;
    });
  }

  createButtonClicked(): void {
    this.router.navigate(['/game-url-products/create']);
  }

  deleteButtonClicked(productId: number, gameUrlId: number): void {
    if (!confirm('Remove product from Game URL?')) {
      return;
    }

    this.gameUrlProductService
      .delete(productId, gameUrlId)
      .subscribe(() => this.fetchGameUrlProducts());
  }

  clearFiltersButtonClicked(): void {
    this.gameIdControl.setValue(null);
    this.gameUrlIdControl.setValue(null);
    this.productIdControl.setValue(null);
    this.loadFilteredGameUrlProducts();
  }

  private applyGameFilter(gameId: number | null): void {
    if (gameId === null) {
      this.gameUrlsFiltered$.next([]);
      this.productsFiltered$.next([]);
      this.gameUrlIdControl.reset();
      this.productIdControl.reset();
      this.loadFilteredGameUrlProducts();
      return;
    }

    const urls = this.gameUrlsAll.filter(u => u.gameId === gameId);
    this.gameUrlsFiltered$.next(urls);

    const products = this.productsAll.filter(p => p.gameId === gameId);
    this.productsFiltered$.next(products);

    this.gameUrlIdControl.reset();
    this.productIdControl.reset();
    this.loadFilteredGameUrlProducts();
  }

  private loadFilteredGameUrlProducts(): void {
    const gameId = this.gameIdControl.value;
    const gameUrlId = this.gameUrlIdControl.value;
    const productId = this.productIdControl.value;

    this.filteredGameUrlProducts = this.gameUrlProducts.filter(x => {
      const matchesGame =
        gameId === null ||
        this.gameUrlsAll.some(
          u => u.id === x.gameUrlId && u.gameId === gameId
        );

      const matchesGameUrl =
        gameUrlId === null || x.gameUrlId === gameUrlId;

      const matchesProduct =
        productId === null || x.productId === productId;

      return matchesGame && matchesGameUrl && matchesProduct;
    });

    this.dataSource.data = this.filteredGameUrlProducts;
  }
}
