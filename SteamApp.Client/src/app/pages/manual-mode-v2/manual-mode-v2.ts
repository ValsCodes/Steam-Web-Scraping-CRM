import {
  ChangeDetectionStrategy,
  ChangeDetectorRef,
  Component,
  OnDestroy,
  OnInit,
} from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormControl, ReactiveFormsModule } from '@angular/forms';
import {
  BehaviorSubject,
  merge,
  startWith,
  Subject,
  takeUntil,
  tap,
} from 'rxjs';
import * as XLSX from 'xlsx';

import { Game, GameUrl, GameUrlProduct, Tag } from '../../models';
import {
  GameService,
  GameUrlProductService,
  GameUrlService,
  TagService,
} from '../../services';
import { CopyLinkComponent, TextFilterComponent } from '../../components';
import { ComboBoxComponent } from '../../components/filter-components/combo-box-filter.component';
import { TagFilterSelectComponent } from '../../components/filter-components/tag-filter.component';
import { CONSTANTS } from '../../common';
import { NumberFilterComponent } from '../../components/filter-components/number-filter.component';

@Component({
  selector: 'steam-manual-mode-v2',
  standalone: true,
  changeDetection: ChangeDetectionStrategy.OnPush,
  imports: [
    CommonModule,
    ReactiveFormsModule,
    CopyLinkComponent,
    TextFilterComponent,
    ComboBoxComponent,
    TagFilterSelectComponent,
    NumberFilterComponent,
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

  readonly searchByRatingFilterControl = new FormControl<number | null>(null, {
    nonNullable: true,
  });

  readonly tagSelectControl = new FormControl<Tag | null>({
    value: null,
    disabled: true,
  });

  gameTagsFilter: Tag[] = [];
  tagsFilter: string[] = [];

  readonly bindToExternal$ = this.gameIdControl.valueChanges.pipe(
    startWith(this.gameIdControl.value),
    tap((gameId) => {
      this.applyGameFilter(gameId);
    }),
  );

  readonly bindGameUrlToExternal$ = this.gameUrlIdControl.valueChanges.pipe(
    startWith(this.gameUrlIdControl.value),
    tap((gameUrlId) => {
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
    }),
  );

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

    merge(this.searchByNameFilterControl.valueChanges)
      .pipe(takeUntil(this.destroy$))
      .subscribe(() => this.loadFilteredProducts());
  }

  ngOnDestroy(): void {
    this.destroy$.next();
    this.destroy$.complete();
  }

  addFilter(value: string): void {
    const v = value.trim().toLowerCase();
    if (v && !this.tagsFilter.includes(v)) {
      this.tagsFilter.push(v);

      this.loadFilteredProducts();
    }
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

  onNameFilterChanged(filter: string): void {
    this.searchByNameFilterControl.setValue(filter, { emitEvent: false });
    this.loadFilteredProducts();
  }

  onRatingFilterChanged(filter: number): void {
    this.searchByRatingFilterControl.setValue(filter, { emitEvent: false });

    this.loadFilteredProducts();
  }

  exportButtonClicked(): void {
    const dataToExport = this.productsFiltered;

    const worksheet: XLSX.WorkSheet = XLSX.utils.json_to_sheet(dataToExport);

    const workbook: XLSX.WorkBook = XLSX.utils.book_new();
    XLSX.utils.book_append_sheet(workbook, worksheet, 'Data');

    const today = new Date();
    XLSX.writeFile(workbook, `Export_${today.toDateString()}_Products.xlsx`);
  }

  clearFiltersButtonClicked(): void {
    this.searchByNameFilterControl.setValue('', { emitEvent: false });
    this.searchByRatingFilterControl.setValue(null, { emitEvent: false });

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
        this.products = products;
        this.productsFiltered = products;
        this.cdr.markForCheck();
      });
  }

  openAllButtonClicked(): void {
    let blocked = false;

    for (const product of this.productsFiltered) {
      const newWindow = window.open(product.fullUrl, '_blank');
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
    const batchSize = this.batchSize ?? 0;
    const toPage = currentIndex + batchSize;

    console.log(this.selectedGameUrl?.isBatchUrl);

    let blocked = false;
    if (this.selectedGameUrl?.isBatchUrl === true) {
      for (; currentIndex < toPage; currentIndex++) {
        if (currentIndex - 1 > toPage || currentIndex < 0) {
          break;
        }

        let url = (this.selectedGameUrl.partialUrl ?? '').replace('{0}', String(currentIndex));

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
          this.products[currentIndex - 1].fullUrl,
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
  }

  clearBatchButtonClicked(): void {
    this.currentIndex = null;
    this.batchSize = null;
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

    this.loadFilteredProducts();
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
        this.gameUrlsAll = urls.map((url) => {
          if (url.isBatchUrl === true) {
            return {
              ...url,
              name: `${url.name} - [ Batch ]`,
            };
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
    this.tagSelectControl.setValue(null);

    if (gameId === null) {
      this.gameUrlsFiltered$.next([]);
      this.gameTagsFilter = [];
      this.tagSelectControl.disable();

      this.gameUrlIdControl.reset();
      this.selectedGameUrl = null;

      this.cdr.markForCheck();
      return;
    }

    const urls = this.gameUrlsAll.filter((url) => url.gameId === gameId);
    this.gameUrlsFiltered$.next(urls);

    this.gameTagsFilter = this.gameTagsAll.filter(
      (tag) => tag.gameId === gameId,
    );
    if (this.gameTagsFilter.length) {
      this.tagSelectControl.enable();
    } else {
      this.tagSelectControl.disable();
    }

    this.gameUrlIdControl.reset();
    this.selectedGameUrl = null;

    this.cdr.markForCheck();
  }

  private loadFilteredProducts(): void {
    const nameFilter = this.searchByNameFilterControl.value.toLowerCase();
    const tagFilters = this.tagsFilter.map((t) => t.toLowerCase());
    const ratingFilter = this.searchByRatingFilterControl.value;

    this.productsFiltered = this.products.filter((product) => {
      const matchesName =
        (!nameFilter ||
          product.productName.toLowerCase().includes(nameFilter)) &&
        (!ratingFilter || product.rating! >= ratingFilter);

      const matchesTags =
        tagFilters.length === 0 ||
        tagFilters.every((filter) =>
          product.tags?.some((tag) => tag.toLowerCase().includes(filter)),
        );

      return matchesName && matchesTags;
    });
  }
}
