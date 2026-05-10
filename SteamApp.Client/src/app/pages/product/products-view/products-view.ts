import { ChangeDetectorRef, Component, OnDestroy, OnInit, ViewChild, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { Router } from '@angular/router';

import { MatTableModule, MatTableDataSource } from '@angular/material/table';
import { MatPaginator, MatPaginatorModule } from '@angular/material/paginator';
import { MatSort, MatSortModule } from '@angular/material/sort';
import { MatDialog } from '@angular/material/dialog';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatMenuModule } from '@angular/material/menu';
import { FormControl, ReactiveFormsModule } from '@angular/forms';
import { finalize, Subject, takeUntil } from 'rxjs';

import { GameService, ProductService, TagService } from '../../../services';
import { Game, Product, Tag, UpdateProductStatus } from '../../../models';
import { ConfirmDialogComponent } from '../../../components/confirm-dialog.component';
import * as XLSX from 'xlsx';

@Component({
  selector: 'steam-products-grid',
  standalone: true,
  imports: [
    CommonModule,
    ReactiveFormsModule,
    MatTableModule,
    MatPaginatorModule,
    MatSortModule,
    MatButtonModule,
    MatIconModule,
    MatMenuModule,
  ],
  templateUrl: './products-view.html',
  styleUrl: './products-view.scss',
})
export class ProductsView implements OnInit, OnDestroy {
  displayedColumns: string[] = [
    'gameName',
    'name',
    'tags',
    'rating',
    'isActive',
    'actions',
  ];

  private readonly destroy$ = new Subject<void>();

  readonly gameIdControl = new FormControl<number | null>(null);
  readonly searchByNameFilterControl = new FormControl<string>('', { nonNullable: true });
  readonly searchByRatingFilterControl = new FormControl<number | null>(null);
  readonly tagIdControl = new FormControl<number | null>({ value: null, disabled: true });
  readonly ratingOptions = [1, 2, 3, 4, 5, 6, 7, 8, 9, 10] as const;

  readonly games = signal<readonly Game[]>([]);
  readonly products = signal<readonly Product[]>([]);
  readonly gameTagsAll = signal<readonly Tag[]>([]);
  readonly gameTagsFilter = signal<readonly Tag[]>([]);
  readonly tagsFilter = signal<readonly string[]>([]);
  private readonly deletingIds = new Set<number>();
  private readonly statusUpdatingIds = new Set<number>();

  dataSource = new MatTableDataSource<Product>([]);
  isGridLoading = false;
  pageSize = 25;
  readonly pageSizeOptions = [10, 25, 50, 100];

  private paginator?: MatPaginator;
  private sort?: MatSort;

  @ViewChild(MatPaginator)
  set matPaginator(paginator: MatPaginator | undefined) {
    this.paginator = paginator;
    this.attachTableControls();
  }

  @ViewChild(MatSort)
  set matSort(sort: MatSort | undefined) {
    this.sort = sort;
    this.attachTableControls();
  }

  constructor(
    private readonly productService: ProductService,
    private readonly gameService: GameService,
    private readonly tagsService: TagService,
    private readonly router: Router,
    private readonly cdr: ChangeDetectorRef,
    private readonly dialog: MatDialog,
  ) {}

  ngOnInit(): void {
    this.loadGames();
    this.loadGameTags();
    this.fetchProducts();

    this.gameIdControl.valueChanges
      .pipe(takeUntil(this.destroy$))
      .subscribe(gameId => this.applyGameFilter(gameId));

    this.searchByNameFilterControl.valueChanges
      .pipe(takeUntil(this.destroy$))
      .subscribe(() => this.loadFilteredProducts());

    this.searchByRatingFilterControl.valueChanges
      .pipe(takeUntil(this.destroy$))
      .subscribe(() => this.loadFilteredProducts());
  }

  ngOnDestroy(): void {
    this.destroy$.next();
    this.destroy$.complete();
  }

  private loadGameTags(): void {
    this.tagsService
      .getAll()
      .pipe(takeUntil(this.destroy$))
      .subscribe(tags => {
        this.gameTagsAll.set(tags);
        this.applyGameFilter(this.gameIdControl.value);
      });
  }

  private loadGames(): void {
    this.gameService
      .getAll()
      .pipe(takeUntil(this.destroy$))
      .subscribe(games => {
        this.games.set(games);
      });
  }

  private applyGameFilter(gameId: number | null): void {
    this.tagsFilter.set([]);
    this.tagIdControl.setValue(null, { emitEvent: false });

    if (gameId === null) {
      this.gameTagsFilter.set([]);
      this.syncTagControlState();
      this.loadFilteredProducts();
      return;
    }

    this.gameTagsFilter.set(this.gameTagsAll().filter(tag => tag.gameId === gameId));
    this.syncTagControlState();
    this.loadFilteredProducts();
  }

  fetchProducts(): void {
    this.isGridLoading = true;
    this.cdr.markForCheck();

    this.productService.getAll()
      .pipe(
        takeUntil(this.destroy$),
        finalize(() => {
          this.isGridLoading = false;
          this.cdr.markForCheck();
        }),
      )
      .subscribe(products => {
        this.products.set(products);
        this.loadFilteredProducts();
        this.attachTableControls();
      });
  }


  exportButtonClicked(): void {
    const dataToExport = this.dataSource.data.map(x => ({
      gameName: x.gameName,
      name: x.name ?? '',
      tags: x.tags?.join(', ') ?? '',
      rating: x.rating ?? '',
      isActive: x.isActive,
    }));

    const worksheet: XLSX.WorkSheet = XLSX.utils.json_to_sheet(dataToExport);
    const workbook: XLSX.WorkBook = XLSX.utils.book_new();
    XLSX.utils.book_append_sheet(workbook, worksheet, 'Products');

    const today = new Date();
    XLSX.writeFile(workbook, `Export_${today.toDateString()}_Products.xlsx`);
  }

  refreshButtonClicked(): void {
    this.fetchProducts();
  }

  pageSizeChanged(value: string | number): void {
    const pageSize = Number(value);
    if (Number.isNaN(pageSize) || pageSize <= 0 || this.pageSize === pageSize) {
      return;
    }

    this.pageSize = pageSize;

    if (this.paginator) {
      this.paginator.pageSize = pageSize;
      this.paginator.firstPage();
      this.dataSource.data = [...this.dataSource.data];
      this.cdr.markForCheck();
    }
  }

  createButtonClicked(): void {
    this.router.navigate(['/products/create']);
  }

  editButtonClicked(id: number): void {
    this.router.navigate(['/products/edit', id]);
  }

  activeButtonClicked(product: Product): void {
    if (this.isStatusUpdating(product.id)) {
      return;
    }

    const nextIsActive = !product.isActive;

    const input: UpdateProductStatus = {
      id: product.id,
      isActive: nextIsActive,
    };

    this.statusUpdatingIds.add(product.id);

    this.productService
      .updateStatus(input)
      .pipe(
        takeUntil(this.destroy$),
        finalize(() => {
          this.statusUpdatingIds.delete(product.id);
        }),
      )
      .subscribe(() => {
        this.products.set(
          this.products().map(p => (p.id === product.id ? { ...p, isActive: nextIsActive } : p)),
        );

        this.loadFilteredProducts();
      });
  }

  deleteButtonClicked(id: number): void {
    if (this.isDeleting(id)) {
      return;
    }

    this.dialog
      .open(ConfirmDialogComponent, {
        width: '420px',
        data: {
          title: 'Delete Product',
          subtitle: 'This action cannot be undone.',
          message: 'Are you sure you want to delete this Product?',
          confirmText: 'Delete',
          cancelText: 'Cancel',
        },
      })
      .afterClosed()
      .pipe(takeUntil(this.destroy$))
      .subscribe((confirmed: boolean) => {
        if (!confirmed) {
          return;
        }

        this.deletingIds.add(id);
        this.cdr.markForCheck();

        this.productService.delete(id)
          .pipe(
            takeUntil(this.destroy$),
            finalize(() => {
              this.deletingIds.delete(id);
              this.cdr.markForCheck();
            }),
          )
          .subscribe({
            next: () => {
              this.fetchProducts();
            },
          });
      });
  }

  isDeleting(id: number): boolean {
    return this.deletingIds.has(id);
  }

  isStatusUpdating(id: number): boolean {
    return this.statusUpdatingIds.has(id);
  }

  onTagSelectedFromSelect(): void {
    const selectedTagId = this.tagIdControl.value;
    if (selectedTagId === null) {
      return;
    }

    const tag = this.gameTagsFilter().find(x => x.id === selectedTagId);
    if (!tag?.name) {
      return;
    }

    const tagName = tag.name.toLowerCase();
    const filters = this.tagsFilter();

    if (!filters.includes(tagName)) {
      this.tagsFilter.set([...filters, tagName]);
    }

    this.gameTagsFilter.set(this.gameTagsFilter().filter(t => t.id !== tag.id));
    this.tagIdControl.setValue(null, { emitEvent: false });
    this.syncTagControlState();

    this.loadFilteredProducts();
  }

  private loadFilteredProducts(): void {
    const gameId = this.gameIdControl.value;
    const nameFilter = this.searchByNameFilterControl.value?.toLowerCase() ?? '';
    const ratingFilter = this.searchByRatingFilterControl.value;
    const tagFilters = this.tagsFilter().map(t => t.toLowerCase());

    const filtered = this.products().filter(product => {
      const matchesGame = gameId === null || product.gameId === gameId;
      const matchesName = !nameFilter || product.name?.toLowerCase().includes(nameFilter);
      const matchesRating = !ratingFilter || product.rating! >= ratingFilter;
      const matchesTags =
        tagFilters.length === 0 ||
        tagFilters.every(filter => product.tags?.some(tag => tag.toLowerCase().includes(filter)));

      return matchesGame && matchesName && matchesRating && matchesTags;
    });

    this.dataSource.data = filtered;
    this.paginator?.firstPage();
  }

  clearFiltersButtonClicked(): void {
    this.searchByNameFilterControl.setValue('', { emitEvent: false });
    this.searchByRatingFilterControl.setValue(null, { emitEvent: false });
    this.gameIdControl.setValue(null, { emitEvent: false });

    this.tagsFilter.set([]);
    this.tagIdControl.setValue(null, { emitEvent: false });
    this.gameTagsFilter.set([]);
    this.syncTagControlState();

    this.loadFilteredProducts();
  }

  removeFilter(value: string): void {
    this.tagsFilter.set(this.tagsFilter().filter(f => f !== value));

    const restored = this.gameTagsAll().find(
      t =>
        t.gameId === this.gameIdControl.value &&
        t.name !== null &&
        t.name.toLowerCase() === value,
    );

    if (restored && !this.gameTagsFilter().some(t => t.id === restored.id)) {
      this.gameTagsFilter.set(
        [...this.gameTagsFilter(), restored].sort((a, b) => (a.name ?? '').localeCompare(b.name ?? '')),
      );
    }

    this.syncTagControlState();
    this.loadFilteredProducts();
  }

  private attachTableControls(): void {
    if (this.paginator) {
      this.dataSource.paginator = this.paginator;
      this.paginator.pageSize = this.pageSize;
    }

    if (this.sort) {
      this.dataSource.sort = this.sort;
    }
  }

  private syncTagControlState(): void {
    if (this.gameTagsFilter().length === 0) {
      this.tagIdControl.disable({ emitEvent: false });
      return;
    }

    this.tagIdControl.enable({ emitEvent: false });
  }

  openInNewTab(id: number): void {
    window.open(`products/edit/${id}`, '_blank');
  }
}
