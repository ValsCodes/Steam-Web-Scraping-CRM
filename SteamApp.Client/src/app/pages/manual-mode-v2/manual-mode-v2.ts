import {
  ChangeDetectionStrategy,
  ChangeDetectorRef,
  Component,
  OnDestroy,
  OnInit,
} from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormControl, ReactiveFormsModule } from '@angular/forms';
import { BehaviorSubject, startWith, Subject, takeUntil } from 'rxjs';
import * as XLSX from 'xlsx';

import { Game, GameUrl, GameUrlProduct, Tag } from '../../models';
import {
  GameService,
  GameUrlProductService,
  GameUrlService,
  TagService,
} from '../../services';
import { CopyLinkComponent } from '../../components';

@Component({
  selector: 'steam-manual-mode-v2',
  standalone: true,
  changeDetection: ChangeDetectionStrategy.OnPush,
  imports: [
    CommonModule,
    ReactiveFormsModule,
    CopyLinkComponent,
  ],
  templateUrl: './manual-mode-v2.html',
  styleUrl: './manual-mode-v2.scss',
})
export class ManualModeV2 implements OnInit, OnDestroy {
  currentIndex: number | null = null;
  batchSize: number | null = null;

  readonly games$ = new BehaviorSubject<readonly Game[]>([]);
  readonly gameUrlsFiltered$ = new BehaviorSubject<readonly GameUrl[]>([]);

  selectedGameUrl: GameUrl | null = null;

  products: GameUrlProduct[] = [];
  productsFiltered: GameUrlProduct[] = [];

  readonly gameIdControl = new FormControl<number | null>(null);
  readonly gameUrlIdControl = new FormControl<number | null>(null);

  readonly searchByNameFilterControl = new FormControl<string>('', {
    nonNullable: true,
  });

  readonly searchByRatingFilterControl = new FormControl<number | null>(null);

  readonly tagSelectControl = new FormControl<Tag | null>({
    value: null,
    disabled: true,
  });

  gameTagsFilter: Tag[] = [];
  tagsFilter: string[] = [];

  private readonly destroy$ = new Subject<void>();

  private games: Game[] = [];
  private gameUrlsAll: GameUrl[] = [];
  private gameTagsAll: Tag[] = [];

  constructor(
    private readonly gameService: GameService,
    private readonly gameUrlService: GameUrlService,
    private readonly gameUrlProductService: GameUrlProductService,
    private readonly tagsService: TagService,
    private readonly cdr: ChangeDetectorRef,
  ) {}

  ngOnInit(): void {
    this.loadGames();
    this.loadGameUrls();
    this.loadGameTags();

    this.gameIdControl.valueChanges
      .pipe(startWith(this.gameIdControl.value), takeUntil(this.destroy$))
      .subscribe((gameId) => {
        this.applyGameFilter(gameId);
      });

    this.gameUrlIdControl.valueChanges
      .pipe(startWith(this.gameUrlIdControl.value), takeUntil(this.destroy$))
      .subscribe((gameUrlId) => {
        if (gameUrlId === null) {
          this.selectedGameUrl = null;
          this.products = [];
          this.productsFiltered = [];
          this.cdr.markForCheck();
          return;
        }

        this.selectedGameUrl =
          this.gameUrlsFiltered$.value.find((u) => u.id === gameUrlId) ?? null;

        this.clearBatchButtonClicked();

        if (this.selectedGameUrl?.isBatchUrl) {
          this.currentIndex = this.selectedGameUrl.startPage ?? null;
          this.batchSize = this.currentIndex === null ? null : 5;
        }

        this.cdr.markForCheck();
      });

    this.searchByNameFilterControl.valueChanges
      .pipe(startWith(this.searchByNameFilterControl.value), takeUntil(this.destroy$))
      .subscribe(() => {
        this.loadFilteredProducts();
        this.cdr.markForCheck();
      });

    this.searchByRatingFilterControl.valueChanges
      .pipe(startWith(this.searchByRatingFilterControl.value), takeUntil(this.destroy$))
      .subscribe(() => {
        this.loadFilteredProducts();
        this.cdr.markForCheck();
      });

    this.tagSelectControl.valueChanges
      .pipe(takeUntil(this.destroy$))
      .subscribe((tag) => {
        if (tag === null) {
          return;
        }
        this.onTagSelectedFromSelect(tag);
      });
  }

  ngOnDestroy(): void {
    this.destroy$.next();
    this.destroy$.complete();
  }

  exportButtonClicked(): void {
    const dataToExport = this.productsFiltered;

    const worksheet: XLSX.WorkSheet = XLSX.utils.json_to_sheet(dataToExport);
    const workbook: XLSX.WorkBook = XLSX.utils.book_new();
    XLSX.utils.book_append_sheet(workbook, worksheet, 'Data');

    const today = new Date();
    XLSX.writeFile(workbook, `Export_${today.toDateString()}_Products.xlsx`);
  }

  clearButtonClicked(): void {
    this.gameIdControl.setValue(null);
    this.gameUrlIdControl.setValue(null);

    this.products = [];
    this.productsFiltered = [];

    this.clearFiltersButtonClicked();
  }

  showAllButtonClicked(): void {
    if (this.selectedGameUrl === null) {
      return;
    }

    this.gameUrlProductService
      .existsByGameUrl(this.selectedGameUrl.id)
      .pipe(takeUntil(this.destroy$))
      .subscribe((products) => {
        const filteredResults = products.filter((x) => x.isActive === true);

        this.products = filteredResults;
        this.productsFiltered = [...filteredResults];

        this.loadFilteredProducts();
        this.cdr.detectChanges();
      });
  }

  openAllButtonClicked(): void {
    let blocked = false;

    for (let i = 0; i < this.productsFiltered.length && i < 5; i++) {
      const newWindow = window.open(this.productsFiltered[i].fullUrl, '_blank');
      if (!newWindow || newWindow.closed) {
        blocked = true;
      }
    }

    if (blocked) {
      alert('Pop-ups were blocked. Please enable pop-ups for this website.');
    }
  }

  startBatchButtonClicked(): void {
    let currentIndex = this.currentIndex ?? 0;

    if (this.batchSize === null) {
      this.batchSize = 0;
    }

    if (this.batchSize > 5) {
      this.batchSize = 5;
    }

    const batchSize = this.batchSize;
    const toPage = currentIndex + batchSize;

    let blocked = false;

    if (this.selectedGameUrl?.isBatchUrl === true) {
      for (; currentIndex < toPage; currentIndex++) {
        if (currentIndex - 1 > toPage || currentIndex < 0) {
          break;
        }

        const url = (this.selectedGameUrl.partialUrl ?? '').replace(
          '{0}',
          String(currentIndex),
        );

        const newWindow = window.open(url, '_blank');

        if (!newWindow || newWindow.closed) {
          blocked = true;
        }
      }
    } else {
      for (; currentIndex < toPage; currentIndex++) {
        if (currentIndex - 1 > this.products.length || currentIndex < 0) {
          break;
        }

        const newWindow = window.open(
          this.productsFiltered[currentIndex - 1].fullUrl,
          '_blank',
        );

        if (!newWindow || newWindow.closed) {
          blocked = true;
        }
      }
    }

    if (blocked) {
      alert('Pop-ups were blocked. Please enable pop-ups for this website.');
    }

    this.currentIndex = currentIndex;
    this.cdr.markForCheck();
  }

  clearBatchButtonClicked(): void {
    this.currentIndex = null;
    this.batchSize = null;
    this.cdr.markForCheck();
  }

  clearFiltersButtonClicked(): void {
    this.searchByNameFilterControl.setValue('', { emitEvent: false });
    this.searchByRatingFilterControl.setValue(null, { emitEvent: false });

    this.tagsFilter = [];

    const gameId = this.gameIdControl.value;
    this.gameTagsFilter =
      gameId === null ? [] : this.gameTagsAll.filter((t) => t.gameId === gameId);

    this.tagSelectControl.setValue(null, { emitEvent: false });
    if (this.gameTagsFilter.length) {
      this.tagSelectControl.enable({ emitEvent: false });
    } else {
      this.tagSelectControl.disable({ emitEvent: false });
    }

    this.loadFilteredProducts();
    this.cdr.markForCheck();
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

    if (this.gameTagsFilter.length) {
      this.tagSelectControl.enable({ emitEvent: false });
    } else {
      this.tagSelectControl.disable({ emitEvent: false });
    }

    this.loadFilteredProducts();
    this.cdr.markForCheck();
  }

  private onTagSelectedFromSelect(tag: Tag): void {
    if (!tag.name) {
      this.tagSelectControl.setValue(null, { emitEvent: false });
      return;
    }

    const tagName = tag.name.toLowerCase();
    if (!this.tagsFilter.includes(tagName)) {
      this.tagsFilter.push(tagName);
    }

    this.gameTagsFilter = this.gameTagsFilter.filter((t) => t.id !== tag.id);

    this.tagSelectControl.setValue(null, { emitEvent: false });

    if (this.gameTagsFilter.length) {
      this.tagSelectControl.enable({ emitEvent: false });
    } else {
      this.tagSelectControl.disable({ emitEvent: false });
    }

    this.loadFilteredProducts();
    this.cdr.markForCheck();
  }

  trackByProductId(_: number, product: GameUrlProduct): number {
    return product.productId;
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

  private loadGameUrls(): void {
    this.gameUrlService
      .getAll()
      .pipe(takeUntil(this.destroy$))
      .subscribe((urls) => {
        this.gameUrlsAll = urls
          .filter((url) => url.isPublicApi === false)
          .map((url) => {
            if (url.isBatchUrl === true) {
              return { ...url, name: `${url.name} - [ Batch ]` };
            }
            return url;
          });

        this.applyGameFilter(this.gameIdControl.value);
      });
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

  private applyGameFilter(gameId: number | null): void {
    this.tagsFilter = [];
    this.tagSelectControl.setValue(null, { emitEvent: false });

    if (gameId === null) {
      this.gameUrlsFiltered$.next([]);
      this.gameTagsFilter = [];
      this.tagSelectControl.disable({ emitEvent: false });

      this.gameUrlIdControl.setValue(null);
      this.selectedGameUrl = null;

      this.products = [];
      this.productsFiltered = [];

      this.cdr.markForCheck();
      return;
    }

    const urls = this.gameUrlsAll.filter((url) => url.gameId === gameId);
    this.gameUrlsFiltered$.next(urls);

    this.gameTagsFilter = this.gameTagsAll.filter((tag) => tag.gameId === gameId);

    if (this.gameTagsFilter.length) {
      this.tagSelectControl.enable({ emitEvent: false });
    } else {
      this.tagSelectControl.disable({ emitEvent: false });
    }

    this.gameUrlIdControl.setValue(null);
    this.selectedGameUrl = null;

    this.products = [];
    this.productsFiltered = [];

    this.cdr.markForCheck();
  }

  private loadFilteredProducts(): void {
    const nameFilter = (this.searchByNameFilterControl.value ?? '').toLowerCase();
    const tagFilters = this.tagsFilter.map((t) => t.toLowerCase());
    const ratingFilter = this.searchByRatingFilterControl.value;

    this.productsFiltered = this.products.filter((product) => {
      const productName = (product.productName ?? '').toLowerCase();
      const productRating = product.rating ?? null;

      const matchesName = !nameFilter || productName.includes(nameFilter);

      const matchesRating =
        ratingFilter === null ||
        ratingFilter === undefined ||
        (productRating !== null && productRating >= ratingFilter);

      const matchesTags =
        tagFilters.length === 0 ||
        tagFilters.every((filter) =>
          product.tags?.some((tag) => tag.toLowerCase().includes(filter)),
        );

      return matchesName && matchesRating && matchesTags;
    });
  }
}