import {
  Component,
  OnInit,
  OnDestroy,
  ChangeDetectionStrategy,
  ChangeDetectorRef,
} from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormControl, ReactiveFormsModule } from '@angular/forms';
import { merge, Subject, takeUntil } from 'rxjs';

import { CONSTANTS } from '../../common/constants';
import { Game, GameUrl, GameUrlProduct, Tag } from '../../models';
import {
  GameService,
  GameUrlProductService,
  GameUrlService,
  TagService,
} from '../../services';
import { CopyLinkComponent, TextFilterComponent } from '../../components';

@Component({
  selector: 'steam-manual-mode-v2',
  standalone: true,
  changeDetection: ChangeDetectionStrategy.OnPush,
  imports: [
    CommonModule,
    ReactiveFormsModule,
    CopyLinkComponent,
    TextFilterComponent,
  ],
  templateUrl: './manual-mode-v2.html',
  styleUrl: './manual-mode-v2.scss',
})
export class ManualModeV2 implements OnInit, OnDestroy {
  currentIndex: number | null = null;
  batchSize: number | null = null;

  games: Game[] = [];
  gameUrlsAll: GameUrl[] = [];
  gameUrlsFiltered: GameUrl[] = [];
  selectedGameUrl: GameUrl | null = null;

  products: GameUrlProduct[] = [];
  productsFiltered: GameUrlProduct[] = [];

  tagSelectControl = new FormControl<Tag | null>({
    value: null,
    disabled: true,
  });

  gameTags: Tag[] = [];
  gameTagsFilter: Tag[] = [];
  tagsFilter: string[] = [];

  gameIdControl = new FormControl<number | null>(null);
  gameUrlIdControl = new FormControl<number | null>(null);

  searchByNameFilterControl = new FormControl<string>('', {
    nonNullable: true,
  });

  private readonly destroy$ = new Subject<void>();

  constructor(
    private readonly gameService: GameService,
    private readonly gameUrlService: GameUrlService,
    private readonly gameUrlProductService: GameUrlProductService,
    private readonly cdr: ChangeDetectorRef,
    private readonly tagsService: TagService,
  ) {}

  ngOnInit(): void {
    this.loadGames();
    this.loadGameUrls();
    this.loadGameTags();

    this.bindGameSelection();
    this.bindGameUrlSelection();

    merge(
      this.searchByNameFilterControl.valueChanges,
    )
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

    const restored = this.gameTags.find(
      (t) =>
        t.gameId === this.gameIdControl.value &&
        t.name !== null &&
        t.name.toLowerCase() === value,
    );

    if (restored && !this.gameTagsFilter.some((t) => t.id === restored.id)) {
      this.gameTagsFilter.push(restored);
    }

    this.loadFilteredProducts();
  }

  onNameFilterChanged(filter: string): void {
    this.searchByNameFilterControl.setValue(filter, { emitEvent: false });
    this.loadFilteredProducts();
  }

  clearFiltersButtonClicked(): void {
    this.searchByNameFilterControl.setValue('', { emitEvent: false });
    this.tagsFilter = [];

    // restore all available tags for current game
    const gameId = this.gameIdControl.value;
    this.gameTagsFilter =
      gameId === null ? [] : this.gameTags.filter((t) => t.gameId === gameId);

    this.tagSelectControl.setValue(null);
    this.gameTagsFilter.length
      ? this.tagSelectControl.enable()
      : this.tagSelectControl.disable();

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

    this.productsFiltered.forEach((product) => {
      const newWindow = window.open(product.fullUrl, '_blank');
      if (!newWindow || newWindow.closed) {
        blocked = true;
      }
    });

    if (blocked) {
      alert('Pop-ups were blocked. Please enable pop-ups for this website.');
    }
  }

  startBatchButtonClicked(): void {
    let currentIndex = this.currentIndex ?? 0;
    const batchSize = this.batchSize ?? 0;
    const toPage = currentIndex + batchSize;

    let blocked = false;

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

    if (blocked) {
      alert('Pop-ups were blocked. Please enable pop-ups for this website.');
    }

    this.currentIndex = currentIndex;
  }

  clearBatchButtonClicked(): void {
    this.currentIndex = null;
    this.batchSize = null;
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

  private loadGameTags(): void {
    this.tagsService
      .getAll()
      .pipe(takeUntil(this.destroy$))
      .subscribe((tags) => {
        this.gameTags = tags;
        this.applyGameFilter(this.gameIdControl.value);
      });
  }

  onTagSelected(): void {
    const tag = this.tagSelectControl.value;
    if (!tag || tag.name === null) {
      return;
    }

    const tagName = tag.name.toLowerCase();

    if (!this.tagsFilter.includes(tagName)) {
      this.tagsFilter.push(tagName);
    }

    this.gameTagsFilter = this.gameTagsFilter.filter((t) => t.id !== tag.id);

    this.tagSelectControl.setValue(null);
    this.gameTagsFilter.length
      ? this.tagSelectControl.enable()
      : this.tagSelectControl.disable();

    this.loadFilteredProducts();
  }

  private bindGameSelection(): void {
    this.gameIdControl.valueChanges
    .pipe(takeUntil(this.destroy$))
    .subscribe(gameId => this.applyGameFilter(gameId));
  }

  private bindGameUrlSelection(): void {
    this.gameUrlIdControl.valueChanges
      .pipe(takeUntil(this.destroy$))
      .subscribe((gameUrlId) => {
        if (gameUrlId === null) {
          this.selectedGameUrl = null;
          this.products = [];
          this.cdr.markForCheck();
          return;
        }

        this.selectedGameUrl =
          this.gameUrlsFiltered.find((u) => u.id === gameUrlId) ?? null;

        this.cdr.markForCheck();
      });
  }

  private applyGameFilter(gameId: number | null): void {
    this.tagsFilter = [];
    this.tagSelectControl.setValue(null);

    if (gameId === null) {
      this.gameUrlsFiltered = [];
      this.gameTagsFilter = [];
      this.tagSelectControl.disable();
      this.gameUrlIdControl.reset();
      this.selectedGameUrl = null;
      this.cdr.markForCheck();
      return;
    }

    this.gameUrlsFiltered = this.gameUrlsAll.filter(
      (url) => url.gameId === gameId,
    );

    this.gameTagsFilter = this.gameTags.filter((tag) => tag.gameId === gameId);

    this.gameTagsFilter.length
      ? this.tagSelectControl.enable()
      : this.tagSelectControl.disable();

    this.gameUrlIdControl.reset();
    this.selectedGameUrl = null;
    this.cdr.markForCheck();
  }

  private loadFilteredProducts(): void {
    const nameFilter =
      this.searchByNameFilterControl.value?.toLowerCase() ?? '';

    const tagFilters = this.tagsFilter.map((t) => t.toLowerCase());

    this.productsFiltered = this.products.filter((product) => {
      const matchesName =
        !nameFilter || product.productName.toLowerCase().includes(nameFilter);

      const matchesTags =
        tagFilters.length === 0 ||
        tagFilters.every((filter) =>
          product.tags?.some((tag) => tag.toLowerCase().includes(filter)),
        );

      return matchesName && matchesTags;
    });
  }

  trackByProductId(_: number, product: GameUrlProduct): number {
    return product.productId;
  }
}
