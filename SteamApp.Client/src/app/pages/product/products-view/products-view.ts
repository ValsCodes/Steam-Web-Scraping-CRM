import { ChangeDetectorRef, Component, OnInit, ViewChild } from '@angular/core';
import { CommonModule } from '@angular/common';
import { Router } from '@angular/router';

import { MatTableModule, MatTableDataSource } from '@angular/material/table';
import { MatPaginator, MatPaginatorModule } from '@angular/material/paginator';
import { MatSort, MatSortModule } from '@angular/material/sort';
import {
  GameService,
  GameUrlService,
  ProductService,
  TagService,
} from '../../../services';
import { Game, Product, Tag } from '../../../models';
import { encode } from '../../../common';
import { FormControl } from '@angular/forms';
import { TextFilterComponent } from '../../../components';
import { NumberFilterComponent } from '../../../components/filters/number-filter.component';
import { TagFilterSelectComponent } from '../../../components/filters/tag-filter.component';
import { BehaviorSubject, startWith, Subject, takeUntil, tap } from 'rxjs';
import { ComboBoxComponent } from '../../../components/filters/combo-box.component';

@Component({
  selector: 'steam-products-grid',
  standalone: true,
  imports: [
    CommonModule,
    MatTableModule,
    MatPaginatorModule,
    MatSortModule,
    TextFilterComponent,
    NumberFilterComponent,
    TagFilterSelectComponent,
    ComboBoxComponent,
  ],
  templateUrl: './products-view.html',
  styleUrl: './products-view.scss',
})
export class ProductsView implements OnInit {
  displayedColumns: string[] = [
    //'id',
    //'gameId',
    'gameName',
    //'fullUrl',
    'name',
    'tags',
    'rating',
    'isActive',
    'actions',
  ];

  private readonly destroy$ = new Subject<void>();

  private games: Game[] = [];

  readonly games$ = new BehaviorSubject<readonly Game[]>([]);
  dataSource = new MatTableDataSource<Product>([]);

  products: Product[] = [];
  productsFiltered: Product[] = [];

  readonly gameIdControl = new FormControl<number | null>(null);

  private gameTagsAll: Tag[] = [];
  gameTagsFilter: Tag[] = [];
  tagsFilter: string[] = [];

  readonly tagSelectControl = new FormControl<Tag | null>({
    value: null,
    disabled: true,
  });

  @ViewChild(MatPaginator) paginator!: MatPaginator;
  @ViewChild(MatSort) sort!: MatSort;

  constructor(
    private productService: ProductService,
    private gameService: GameService,
    private tagsService: TagService,
    private router: Router,
    private readonly cdr: ChangeDetectorRef,
  ) {}

  ngOnInit(): void {
    this.loadGames();
    this.loadGameTags();

    this.fetchProducts();
  }

  readonly bindToExternal$ = this.gameIdControl.valueChanges.pipe(
    startWith(this.gameIdControl.value),
    tap((gameId) => {
      this.applyGameFilter(gameId);
    }),
  );

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
    this.gameService
      .getAll()
      .pipe(takeUntil(this.destroy$))
      .subscribe((games) => {
        this.games = games;
        this.games$.next(games);
        this.cdr.markForCheck();
      });
  }

  private applyGameFilter(gameId: number | null): void {
    this.tagsFilter = [];
    this.tagSelectControl.setValue(null);

    if (gameId === null) {
      this.gameTagsFilter = [];
      this.tagSelectControl.disable();

      this.cdr.markForCheck();
      return;
    }

    this.gameTagsFilter = this.gameTagsAll.filter(
      (tag) => tag.gameId === gameId,
    );
    if (this.gameTagsFilter.length) {
      this.tagSelectControl.enable();
    } else {
      this.tagSelectControl.disable();
    }

    this.cdr.markForCheck();
  }

  fetchProducts(): void {
    this.productService.getAll().subscribe((products) => {
      this.dataSource.data = products;
      this.dataSource.paginator = this.paginator;
      this.dataSource.sort = this.sort;

      this.products = products;
      this.productsFiltered = products;
    });
  }

  createButtonClicked(): void {
    this.router.navigate(['/products/create']);
  }

  encodeText(value: string): string {
    return encode(value);
  }

  editButtonClicked(id: number): void {
    this.router.navigate(['/products/edit', id]);
  }

  deleteButtonClicked(id: number): void {
    if (!confirm('Delete this product?')) {
      return;
    }

    this.productService.delete(id).subscribe(() => {
      this.fetchProducts();
    });
  }

  searchByNameFilterControl = new FormControl<string>('', {
    nonNullable: true,
  });

  searchByRatingFilterControl = new FormControl<number | null>(null, {
    nonNullable: true,
  });

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

    this.loadFilteredProducts();
  }

  onNameFilterChanged(filter: string): void {
    this.searchByNameFilterControl.setValue(filter, { emitEvent: false });
    this.loadFilteredProducts();
  }

  onRatingFilterChanged(filter: number): void {
    this.searchByRatingFilterControl.setValue(filter, { emitEvent: false });
    this.loadFilteredProducts();
  }

  private loadFilteredProducts(): void {
    const nameFilter =
      this.searchByNameFilterControl.value?.toLowerCase() ?? '';

    const ratingFilter = this.searchByRatingFilterControl.value;
    const tagFilters = this.tagsFilter.map((t) => t.toLowerCase());

    this.productsFiltered = this.products.filter((product) => {
      const matchesName =
        (!nameFilter || product.name?.toLowerCase().includes(nameFilter)) &&
        (!ratingFilter || product.rating == ratingFilter);

      const matchesTags =
        tagFilters.length === 0 ||
        tagFilters.every((filter) =>
          product.tags?.some((tag) => tag.toLowerCase().includes(filter)),
        );

      return matchesName && matchesTags;
    });

    this.dataSource.data = this.productsFiltered;
  }

  clearFiltersButtonClicked(): void {
    this.searchByNameFilterControl.setValue('', { emitEvent: false });
    this.searchByRatingFilterControl.setValue(null, { emitEvent: false });

    this.gameIdControl.setValue(null);

    this.searchByNameFilterControl.setValue('', { emitEvent: false });
    this.tagsFilter = [];

    const gameId = this.gameIdControl.value;
    this.gameTagsFilter =
      gameId === null
        ? []
        : this.gameTagsAll.filter((t) => t.gameId === gameId);

    this.tagSelectControl.setValue(null);
    if (this.gameTagsFilter.length) {
      this.tagSelectControl.enable();
    } else {
      this.tagSelectControl.disable();
    }

    this.loadFilteredProducts();
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

    this.loadFilteredProducts();
  }
}
