import { AfterViewInit, ChangeDetectorRef, Component, OnInit, ViewChild } from '@angular/core';
import { CommonModule } from '@angular/common';
import { Router } from '@angular/router';
import { FormControl } from '@angular/forms';
import { BehaviorSubject, startWith, Subject, takeUntil, tap } from 'rxjs';

import { MatTableModule, MatTableDataSource } from '@angular/material/table';
import { MatPaginator, MatPaginatorModule } from '@angular/material/paginator';
import { MatSort, MatSortModule } from '@angular/material/sort';

import { Game, GameUrl, Product, GameUrlProduct, Tag } from '../../../models';
import {
  GameService,
  GameUrlService,
  ProductService,
  GameUrlProductService,
  TagService,
} from '../../../services';
import { ComboBoxComponent } from '../../../components/filter-components/combo-box-filter.component';
import { TagFilterSelectComponent } from "../../../components/filter-components/tag-filter.component";

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
    TagFilterSelectComponent
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
 private readonly destroy$ = new Subject<void>();

  gameUrlProducts: GameUrlProduct[] = [];
  filteredGameUrlProducts: GameUrlProduct[] = [];

  readonly games$ = new BehaviorSubject<readonly Game[]>([]);
  readonly gameUrlsFiltered$ = new BehaviorSubject<readonly GameUrl[]>([]);
  readonly productsFiltered$ = new BehaviorSubject<readonly Product[]>([]);
  

  readonly gameIdControl = new FormControl<number | null>(null);
  readonly gameUrlIdControl = new FormControl<number | null>(null);
  readonly productIdControl = new FormControl<number | null>(null);

  private gameTagsAll: Tag[] = [];
  gameTagsFilter: Tag[] = [];
  tagsFilter: string[] = [];
    readonly tagSelectControl = new FormControl<Tag | null>({
    value: null,
    disabled: true,
  });

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
    private tagsService: TagService,
    private readonly cdr: ChangeDetectorRef,
  ) {}

  ngOnInit(): void {
    this.loadGames();
    this.loadGameUrls();
    this.loadProducts();
    this.loadGameTags();

    this.fetchGameUrlProducts();
  }

  ngAfterViewInit(): void {
    this.dataSource.paginator = this.paginator;
    this.dataSource.sort = this.sort;
  }

    private loadGameTags(): void {
      this.tagsService
        .getAll()
        .pipe(takeUntil(this.destroy$))
        .subscribe((tags) => {
          this.gameTagsAll = tags;
          this.applyGameFilter(this.gameIdControl.value);
        });
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
    .subscribe(() => {
      this.gameUrlProducts = this.gameUrlProducts.filter(x => {
        return !(x.productId === productId && x.gameUrlId === gameUrlId);
      });

      this.loadFilteredGameUrlProducts();
    });
}

  clearFiltersButtonClicked(): void {
    this.gameIdControl.setValue(null);
    this.gameUrlIdControl.setValue(null);
    this.productIdControl.setValue(null);
    this.tagsFilter = [];
    this.loadFilteredGameUrlProducts();
  }

  private applyGameFilter(gameId: number | null): void {
    if (gameId === null) {
      this.gameUrlsFiltered$.next([]);
      this.productsFiltered$.next([]);
      this.gameUrlIdControl.reset();
      this.productIdControl.reset();
      this.gameTagsFilter = [];
      this.tagSelectControl.disable();
      this.loadFilteredGameUrlProducts();
      return;
    }

    const urls = this.gameUrlsAll.filter(u => u.gameId === gameId);
    this.gameUrlsFiltered$.next(urls);

    const products = this.productsAll.filter(p => p.gameId === gameId);
    this.productsFiltered$.next(products);

        this.gameTagsFilter = this.gameTagsAll.filter(
      (tag) => tag.gameId === gameId,
    );
    if (this.gameTagsFilter.length) {
      this.tagSelectControl.enable();
    } else {
      this.tagSelectControl.disable();
    }

    this.gameUrlIdControl.reset();
    this.productIdControl.reset();
    this.loadFilteredGameUrlProducts();
  }

  private loadFilteredGameUrlProducts(): void {
    const gameId = this.gameIdControl.value;
    const gameUrlId = this.gameUrlIdControl.value;
    const productId = this.productIdControl.value;
    const tagFilters = this.tagsFilter.map((t) => t.toLowerCase());

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

              const matchesTags =
        tagFilters.length === 0 ||
        tagFilters.every((filter) =>
          x.tags?.some((tag) => tag.toLowerCase().includes(filter)),
        );


      return matchesGame && matchesGameUrl && matchesProduct && matchesTags; 
    });

    this.dataSource.data = this.filteredGameUrlProducts;
  }

    onTagSelectedFromComponent(tag: Tag): void {
    if (!tag.name) {
      return;
    }

    const tagName = tag.name.toLowerCase();
    if (!this.tagsFilter.includes(tagName)) {
      this.tagsFilter.push(tagName);
    }

    this.gameTagsFilter = this.gameTagsFilter.filter((t) => t.id !== tag.id);
    if (this.gameTagsFilter.length) {
      this.tagSelectControl.enable();
    } else {
      this.tagSelectControl.disable();
    }

    this.loadFilteredGameUrlProducts();
  }

   removeFilter(value: string): void {
    this.tagsFilter = this.tagsFilter.filter((f) => f !== value);

    const restored = this.gameTagsAll.find(
      (t) =>
        t.gameId === this.gameIdControl.value &&
        t.name !== null &&
        t.name.toLowerCase() === value,
    );

    if (restored && !this.gameTagsFilter.some((t) => t.id === restored.id)) {
      this.gameTagsFilter.push(restored);
      this.gameTagsFilter.sort((a, b) =>
        (a.name ?? '').localeCompare(b.name ?? ''),
      );
    }

    this.loadFilteredGameUrlProducts();
  }

  openInNewTab(id: number): void {
  window.open(`products/edit/${id}`, '_blank');
}
}
