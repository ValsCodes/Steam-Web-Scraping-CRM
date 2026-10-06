import {
  ChangeDetectionStrategy,
  ChangeDetectorRef,
  Component,
  OnDestroy,
  OnInit,
} from '@angular/core';
import { CommonModule } from '@angular/common';
import { HttpErrorResponse } from '@angular/common/http';
import { FormControl, FormsModule, ReactiveFormsModule } from '@angular/forms';
import {
  BehaviorSubject,
  finalize,
  Observable,
  startWith,
  Subject,
  switchMap,
  takeUntil,
  takeWhile,
  timer,
} from 'rxjs';
import * as XLSX from 'xlsx';

import {
  Game,
  GameUrl,
  GameUrlProduct,
  GameUrlProductCurrentStock,
  ManualCheckProductError,
  ManualCheckProductResult,
  ManualCheckProductTrace,
  ManualCheckPresetCombinationWrite,
  ManualCheckRunDetail,
  ScrapingMode,
  ScrapingModeEnum,
  Tag,
} from '../../models';
import {
  ExternalLinkDisclosureService,
  GameService,
  GameUrlProductService,
  GameUrlService,
  ManualCheckService,
  ScrapingModeService,
  TagService,
} from '../../services';
import { MatTooltip } from "@angular/material/tooltip";
import { MatDialog, MatDialogModule } from '@angular/material/dialog';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatMenuModule } from '@angular/material/menu';
import {
  ExternalLinkDirective,
  externalUrlWarning,
  formatDuration,
  openableExternalUrl,
  openableSteamUrl,
} from '../../common';
import {
  ManualCheckSetupDialogComponent,
  ManualCheckSetupDialogData,
  ManualCheckSetupDialogResult,
} from './manual-check-setup-dialog.component';
import {
  ManualCheckHistoryDialogComponent,
} from './manual-check-history-dialog.component';
import {
  ManualCheckTraceDialogComponent,
  ManualCheckTraceDialogData,
  ManualCheckTraceView,
} from './manual-check-trace-dialog.component';
import { ManualCheckSteamResultDialogComponent } from './manual-check-steam-result-dialog.component';
import { ManualCheckMatchTreeComponent } from './manual-check-match-tree.component';
import {
  ManualCheckMatchesDialogComponent,
  ManualCheckMatchesDialogData,
} from './manual-check-matches-dialog.component';
import { groupByItemGroup, ItemGroupSection } from '../../common/item-grouping';
import {
  AdvancedStockDialogComponent,
  AdvancedStockDialogData,
} from './advanced-stock-dialog.component';

type StockStateFilter = 'all' | 'negative' | 'zero' | 'positive';
type ProductSort =
  | 'default'
  | 'price-ascending'
  | 'price-descending'
  | 'stock-ascending'
  | 'stock-descending'
  | 'name-ascending'
  | 'name-descending';
type AutomatedOutcomeFilter = 'All' | 'Matched' | 'Failed' | 'Pending' | 'No match';

@Component({
  selector: 'steam-manual-mode-v2',
  standalone: true,
  changeDetection: ChangeDetectionStrategy.OnPush,
  imports: [
    CommonModule,
    FormsModule,
    ReactiveFormsModule,
    MatTooltip,
    MatDialogModule,
    MatButtonModule,
    MatIconModule,
    MatMenuModule,
    ExternalLinkDirective,
    ManualCheckMatchTreeComponent,
],
  templateUrl: './manual-mode-v2.html',
  styleUrl: './manual-mode-v2.scss',
  host: {
    '(document:keydown)': 'onProductPreviewKey($event)',
    '(document:keyup)': 'onProductPreviewKey($event)',
    '(window:blur)': 'onProductPreviewBlur()',
  },
})
export class ManualModeV2 implements OnInit, OnDestroy {
  readonly formatDuration = formatDuration;
  readonly externalUrlWarning = externalUrlWarning;
  readonly openableExternalUrl = openableExternalUrl;

  currentIndex: number | null = 1;
  batchSize: number | null = 1;
  private openInSteamModeValue = false;

  get openInSteamMode(): boolean {
    return this.openInSteamModeValue;
  }

  set openInSteamMode(enabled: boolean) {
    this.openInSteamModeValue = enabled;
    if (enabled) {
      this.batchSize = 1;
    }
  }

  readonly ratingOptions = [1, 2, 3, 4, 5, 6, 7, 8, 9, 10] as const;

  readonly games$ = new BehaviorSubject<readonly Game[]>([]);
  readonly scrapingModes$ = new BehaviorSubject<readonly ScrapingMode[]>([]);
  readonly gameUrlsFiltered$ = new BehaviorSubject<readonly GameUrl[]>([]);

  selectedGameUrl: GameUrl | null = null;

  products: GameUrlProduct[] = [];
  productsFiltered: GameUrlProduct[] = [];
  automatedRun: ManualCheckRunDetail | null = null;
  automatedRunId: number | null = null;
  automatedRunActive = false;
  automatedCancelPending = false;
  automatedPausePending = false;
  automatedContinuePending = false;
  automatedListingLimitDraft: number | null = null;
  automatedListingLimitDirty = false;
  automatedListingLimitPending = false;
  hasAutomatedCheckResult = false;
  automatedOutcomeFilter: AutomatedOutcomeFilter = 'All';
  automatedCheckWarning = '';
  automatedCheckError = '';
  productRelationError = '';
  readonly selectedProductIds = new Set<number>();
  readonly removedProductIds = new Set<number>();
  readonly productRelationUpdatingIds = new Set<number>();
  private productSelectionAnchor: number | null = null;

  readonly productPreviewIds = new Set<number>();
  private hoveredProductId: number | null = null;
  private productPreviewShiftHeld = false;

  onProductHover(productId: number, event: MouseEvent): void {
    this.hoveredProductId = productId;
    this.productPreviewShiftHeld = event.shiftKey;
    this.updateProductPreview();
  }

  onProductPreviewKey(event: KeyboardEvent): void {
    this.productPreviewShiftHeld = event.shiftKey;
    this.updateProductPreview();
  }

  clearProductPreview(): void {
    this.hoveredProductId = null;
    this.productPreviewIds.clear();
    this.cdr.markForCheck();
  }

  onProductPreviewBlur(): void {
    this.productPreviewShiftHeld = false;
    this.clearProductPreview();
  }

  private updateProductPreview(): void {
    this.productPreviewIds.clear();
    if (this.productPreviewShiftHeld && this.hoveredProductId !== null && this.productSelectionAnchor !== null) {
      const visible = this.productsFiltered;
      const anchorIndex = visible.findIndex(product => product.productId === this.productSelectionAnchor);
      const endpointIndex = visible.findIndex(product => product.productId === this.hoveredProductId);
      if (anchorIndex >= 0 && endpointIndex >= 0) {
        for (const product of visible.slice(Math.min(anchorIndex, endpointIndex), Math.max(anchorIndex, endpointIndex) + 1)) {
          this.productPreviewIds.add(product.productId);
        }
      }
    }
    this.cdr.markForCheck();
  }


  readonly gameIdControl = new FormControl<number | null>(null);
  readonly scrapingModeIdControl = new FormControl<number | null>(null);
  readonly gameUrlIdControl = new FormControl<number | null>(null);

  readonly searchByNameFilterControl = new FormControl<string>('', {
    nonNullable: true,
  });

  readonly searchByRatingFilterControl = new FormControl<number | null>(null);
  readonly searchByStockFilterControl = new FormControl<number | null>(null);
  readonly stockStateFilterControl = new FormControl<StockStateFilter>('all', { nonNullable: true });
  readonly productSortControl = new FormControl<ProductSort>('default', { nonNullable: true });

  readonly tagSelectControl = new FormControl<Tag | null>({
    value: null,
    disabled: true,
  });

  gameTagsFilter: Tag[] = [];
  tagsFilter: string[] = [];

  private readonly destroy$ = new Subject<void>();
  private readonly automatedRunStop$ = new Subject<void>();

  private games: Game[] = [];
  private gameUrlsAll: GameUrl[] = [];
  private gameTagsAll: Tag[] = [];
  private readonly automatedMatches = new Map<number, ManualCheckProductResult>();
  private readonly automatedProductTraces = new Map<number, ManualCheckProductTrace>();
  private readonly automatedProductErrors = new Map<number, ManualCheckProductError>();
  private readonly automatedTargetProductIds = new Set<number>();
  private readonly lastPresetByGameUrl = new Map<number, number>();
  readonly stockUpdatingProductIds = new Set<number>();

  constructor(
    private readonly gameService: GameService,
    private readonly gameUrlService: GameUrlService,
    private readonly gameUrlProductService: GameUrlProductService,
    private readonly scrapingModeService: ScrapingModeService,
    private readonly tagsService: TagService,
    private readonly manualCheckService: ManualCheckService,
    private readonly dialog: MatDialog,
    private readonly cdr: ChangeDetectorRef,
    private readonly externalLinkDisclosure: ExternalLinkDisclosureService,
  ) {}

  get gameTagGroups(): readonly ItemGroupSection<Tag>[] {
    return groupByItemGroup(this.gameTagsFilter);
  }

  get gameUrlGroups(): readonly ItemGroupSection<GameUrl>[] {
    return groupByItemGroup(this.gameUrlsFiltered$.value);
  }

  ngOnInit(): void {
    this.loadGames();
    this.loadScrapingModes();
    this.loadGameUrls();
    this.loadGameTags();

    this.gameIdControl.valueChanges
      .pipe(startWith(this.gameIdControl.value), takeUntil(this.destroy$))
      .subscribe((gameId) => {
        this.applySourceFilters(gameId, this.scrapingModeIdControl.value);
      });

    this.scrapingModeIdControl.valueChanges
      .pipe(startWith(this.scrapingModeIdControl.value), takeUntil(this.destroy$))
      .subscribe((scrapingModeId) => {
        this.applySourceFilters(this.gameIdControl.value, scrapingModeId);
      });

    this.gameUrlIdControl.valueChanges
      .pipe(startWith(this.gameUrlIdControl.value), takeUntil(this.destroy$))
      .subscribe((gameUrlId) => {
        if (gameUrlId === null) {
          this.selectedGameUrl = null;
          this.products = [];
          this.productsFiltered = [];
          this.resetProductRelationState();
          this.clearProductSelection();
          this.resetAutomatedCheck();
          this.cdr.markForCheck();
          return;
        }

        this.selectedGameUrl =
          this.gameUrlsFiltered$.value.find((u) => u.id === gameUrlId) ?? null;

        this.clearBatchButtonClicked();
        this.resetProductRelationState();
        this.clearProductSelection();
        this.resetAutomatedCheck();

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

    this.searchByStockFilterControl.valueChanges
      .pipe(startWith(this.searchByStockFilterControl.value), takeUntil(this.destroy$))
      .subscribe((stock) => {
        if (stock !== null) {
          this.stockStateFilterControl.setValue('all', { emitEvent: false });
        }
        this.loadFilteredProducts();
        this.cdr.markForCheck();
      });

    this.stockStateFilterControl.valueChanges
      .pipe(takeUntil(this.destroy$))
      .subscribe((state) => {
        if (state !== 'all') {
          this.searchByStockFilterControl.setValue(null, { emitEvent: false });
        }
        this.loadFilteredProducts();
        this.cdr.markForCheck();
      });

    this.productSortControl.valueChanges
      .pipe(takeUntil(this.destroy$))
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
    this.automatedRunStop$.next();
    this.automatedRunStop$.complete();
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
    this.scrapingModeIdControl.setValue(null);
    this.gameUrlIdControl.setValue(null);

    this.products = [];
    this.productsFiltered = [];
    this.resetProductRelationState();
    this.clearProductSelection();
    this.resetAutomatedCheck();

    this.clearFiltersButtonClicked();
  }

  showAllButtonClicked(): void {
    if (this.selectedGameUrl === null) {
      return;
    }

    this.resetAutomatedCheck();
    this.loadProductsGrid(this.selectedGameUrl.id);
  }

  automatedCheckButtonClicked(): void {
    this.openAutomatedCheckSetup(null);
  }

  automatedCheckSelectedProducts(): void {
    const productIds = this.products
      .filter((product) => this.selectedProductIds.has(product.productId))
      .map((product) => product.productId);
    if (productIds.length === 0) {
      return;
    }

    this.openAutomatedCheckSetup(productIds);
  }

  automatedCheckProduct(product: GameUrlProduct): void {
    this.openAutomatedCheckSetup([product.productId]);
  }

  incrementCurrentStock(product: GameUrlProduct): void {
    if (this.isStockUpdating(product.productId)) { return; }
    this.updateProductStock(
      product.productId,
      this.gameUrlProductService.incrementCurrentStock(product.productId, product.gameUrlId),
    );
  }

  decrementCurrentStock(product: GameUrlProduct): void {
    if (this.isStockUpdating(product.productId)) { return; }
    this.updateProductStock(
      product.productId,
      this.gameUrlProductService.decrementCurrentStock(product.productId, product.gameUrlId),
    );
  }

  isStockUpdating(productId: number): boolean {
    return this.stockUpdatingProductIds.has(productId);
  }

  openAdvancedStock(product: GameUrlProduct): void {
    const data: AdvancedStockDialogData = {
      productId: product.productId,
      gameUrlId: product.gameUrlId,
      productName: product.productName,
      currentStock: product.currentStock,
    };
    this.dialog.open<AdvancedStockDialogComponent, AdvancedStockDialogData, number>(
      AdvancedStockDialogComponent,
      { data, width: 'min(38rem, 96vw)', maxWidth: '96vw', maxHeight: '92vh' },
    ).afterClosed().pipe(takeUntil(this.destroy$)).subscribe((currentStock) => {
      if (currentStock !== undefined) {
        this.applyCurrentStock(product.productId, currentStock);
      }
    });
  }

  private openAutomatedCheckSetup(productIds: number[] | null): void {
    const source = this.selectedGameUrl;
    const gameId = this.gameIdControl.value;
    if (
      !source ||
      gameId === null ||
      !this.isAutomatedCheckEligible(source) ||
      this.automatedRunActive
    ) {
      return;
    }

    const gameName = this.games.find((x) => x.id === gameId)?.name ?? `Game #${gameId}`;
    const data: ManualCheckSetupDialogData = {
      gameId,
      gameUrlId: source.id,
      gameName,
      gameUrlName: source.name ?? `Game URL #${source.id}`,
      gameUrls: this.gameUrlsAll.filter((x) => x.gameId === gameId),
      preselectedPresetId: this.lastPresetByGameUrl.get(source.id),
    };

    this.dialog.open<ManualCheckSetupDialogComponent, ManualCheckSetupDialogData, ManualCheckSetupDialogResult>(
      ManualCheckSetupDialogComponent,
      {
        data,
        width: 'min(76rem, 96vw)',
        maxWidth: '96vw',
        maxHeight: '92vh',
        panelClass: 'manual-check-setup-dialog-panel',
        disableClose: true,
      },
    ).afterClosed().pipe(takeUntil(this.destroy$)).subscribe((result) => {
      if (result === undefined || result.mode !== 'run') {
        return;
      }
      if (result.presetId !== null) {
        this.lastPresetByGameUrl.set(source.id, result.presetId);
      }
      this.startAutomatedRun(
        source.id,
        result.presetId,
        result.presetCombination ?? null,
        result.bypassCache,
        productIds,
      );
    });
  }

  automatedCheckHistoryButtonClicked(): void {
    this.dialog.open<ManualCheckHistoryDialogComponent, object, number>(
      ManualCheckHistoryDialogComponent,
      { data: {}, width: 'min(76rem, 96vw)', maxWidth: '96vw' },
    ).afterClosed().pipe(takeUntil(this.destroy$)).subscribe((runId) => {
      if (runId !== undefined) {
        this.pollAutomatedRun(runId);
      }
    });
  }

  get canViewAutomatedResult(): boolean {
    return this.automatedRun !== null && this.isTerminalStatus(this.automatedRun);
  }

  openAutomatedResult(): void {
    const detail = this.automatedRun;
    if (!detail || !this.isTerminalStatus(detail)) {
      return;
    }

    const initialView: ManualCheckTraceView = detail.failedProducts > 0 || detail.status === 'Failed'
      ? 'errors'
      : 'results';
    const data: ManualCheckTraceDialogData = { detail, initialView };
    this.dialog.open<ManualCheckTraceDialogComponent, ManualCheckTraceDialogData>(
      ManualCheckTraceDialogComponent,
      {
        width: 'min(68rem, 96vw)',
        maxWidth: '96vw',
        maxHeight: '92vh',
        data,
      },
    );
  }

  setAutomatedOutcomeFilter(filter: AutomatedOutcomeFilter): void {
    this.automatedOutcomeFilter = filter;
    this.loadFilteredProducts();
    this.cdr.markForCheck();
  }

  isAutomatedCheckEligible(gameUrl: GameUrl | null): boolean {
    return !!gameUrl?.isActive && gameUrl.scrapingModeId === ScrapingModeEnum.ManualBatch;
  }

  getAutomatedMatch(productId: number): ManualCheckProductResult | null {
    return this.automatedMatches.get(productId) ?? null;
  }

  getAutomatedProductError(productId: number): ManualCheckProductError | null {
    return this.automatedProductErrors.get(productId) ?? null;
  }

  getAutomatedSteamApiResult(productId: number): ManualCheckProductTrace | null {
    const trace = this.automatedProductTraces.get(productId);
    return trace?.steamApiResultJson ? trace : null;
  }

  getAutomatedPriceTrace(productId: number): ManualCheckProductTrace | null {
    const trace = this.automatedProductTraces.get(productId);
    return trace?.lowestCheckedPriceMinorUnits == null ? null : trace;
  }

  canInspectAutomatedListings(productId: number): boolean {
    return this.automatedProductTraces.get(productId)?.matchEvaluated === true;
  }

  openAutomatedMatches(productId: number): void {
    const match = this.getAutomatedMatch(productId);
    const trace = this.automatedProductTraces.get(productId) ?? null;
    if (!trace?.matchEvaluated) {
      return;
    }

    const data: ManualCheckMatchesDialogData = {
      productName: trace.productName,
      match,
      criteria: this.automatedRun?.setup.criteria ?? [],
      trace,
    };
    this.dialog.open<ManualCheckMatchesDialogComponent, ManualCheckMatchesDialogData>(
      ManualCheckMatchesDialogComponent,
      {
        data,
        width: 'min(48rem, 96vw)',
        maxWidth: '96vw',
        maxHeight: '92vh',
      },
    );
  }

  formatAutomatedPrice(trace: ManualCheckProductTrace): string {
    return this.formatPriceMinorUnits(
      trace.lowestCheckedPriceMinorUnits,
      trace.priceCurrencyCode,
    );
  }

  private formatPriceMinorUnits(
    priceMinorUnits: number | null | undefined,
    currencyCode: string | null | undefined,
  ): string {
    if (priceMinorUnits == null) {
      return 'Price unavailable';
    }

    return new Intl.NumberFormat('en-US', {
      style: 'currency',
      currency: currencyCode || 'EUR',
      minimumFractionDigits: 2,
      maximumFractionDigits: 2,
    }).format(priceMinorUnits / 100);
  }

  openAutomatedSteamApiResult(productId: number): void {
    const trace = this.getAutomatedSteamApiResult(productId);
    if (!trace) {
      return;
    }

    this.dialog.open(ManualCheckSteamResultDialogComponent, {
      width: '64rem',
      maxWidth: '96vw',
      maxHeight: '90vh',
      data: trace,
    });
  }

  getAutomatedProductOutcome(
    productId: number,
  ): 'Pending' | 'Matched' | 'No match' | 'Failed' | null {
    if (this.automatedProductErrors.has(productId)) {
      return 'Failed';
    }

    if (this.automatedMatches.has(productId)) {
      return 'Matched';
    }

    if (!this.automatedTargetProductIds.has(productId)) {
      return null;
    }

    const trace = this.automatedProductTraces.get(productId);
    return trace?.matchEvaluated ? 'No match' : 'Pending';
  }

  get selectedProductCount(): number {
    return this.selectedProductIds.size;
  }

  get areAllFilteredProductsSelected(): boolean {
    const selectableProducts = this.productsFiltered.filter(
      (product) => !this.isProductRemoved(product.productId) && !this.isProductRelationUpdating(product.productId),
    );
    return selectableProducts.length > 0 &&
      selectableProducts.every((product) => this.selectedProductIds.has(product.productId));
  }

  get areSomeFilteredProductsSelected(): boolean {
    const selectableProductCount = this.productsFiltered.filter(
      (product) => !this.isProductRemoved(product.productId) && !this.isProductRelationUpdating(product.productId),
    ).length;
    const selectedCount = this.productsFiltered.filter(
      (product) => !this.isProductRemoved(product.productId) &&
        !this.isProductRelationUpdating(product.productId) &&
        this.selectedProductIds.has(product.productId),
    ).length;
    return selectedCount > 0 && selectedCount < selectableProductCount;
  }

  isProductSelected(productId: number): boolean {
    return this.selectedProductIds.has(productId);
  }

  isProductRemoved(productId: number): boolean {
    return this.removedProductIds.has(productId);
  }

  isProductRelationUpdating(productId: number): boolean {
    return this.productRelationUpdatingIds.has(productId);
  }

  toggleGameUrlProduct(product: GameUrlProduct): void {
    const gameUrlId = this.selectedGameUrl?.id;
    if (gameUrlId !== product.gameUrlId || this.isProductRelationUpdating(product.productId)) {
      return;
    }

    const isRemoved = this.isProductRemoved(product.productId);
    this.productRelationError = '';
    this.productRelationUpdatingIds.add(product.productId);
    this.cdr.markForCheck();

    this.gameUrlProductService.bulkUpdate(
      gameUrlId,
      isRemoved ? [product.productId] : [],
      isRemoved ? [] : [product.productId],
    ).pipe(
      takeUntil(this.destroy$),
      finalize(() => {
        this.productRelationUpdatingIds.delete(product.productId);
        this.cdr.markForCheck();
      }),
    ).subscribe({
      next: () => {
        if (this.selectedGameUrl?.id !== gameUrlId) {
          return;
        }

        if (isRemoved) {
          this.removedProductIds.delete(product.productId);
        } else {
          this.removedProductIds.add(product.productId);
          this.selectedProductIds.delete(product.productId);
        }
        this.loadFilteredProducts();
        this.cdr.markForCheck();
      },
      error: (error) => {
        if (this.selectedGameUrl?.id === gameUrlId) {
          this.productRelationError = this.getRequestError(
            error,
            'Unable to update the Game URL product.',
          );
          this.cdr.markForCheck();
        }
      },
    });
  }

  removeSelectedProductsFromGameUrl(): void {
    const gameUrlId = this.selectedGameUrl?.id;
    if (gameUrlId === undefined || this.productRelationUpdatingIds.size > 0) {
      return;
    }

    const productIds = this.products
      .filter((product) =>
        product.gameUrlId === gameUrlId &&
        this.selectedProductIds.has(product.productId) &&
        !this.isProductRemoved(product.productId))
      .map((product) => product.productId);
    if (productIds.length === 0) {
      return;
    }

    this.productRelationError = '';
    for (const productId of productIds) {
      this.productRelationUpdatingIds.add(productId);
    }
    this.cdr.markForCheck();

    this.gameUrlProductService.bulkUpdate(gameUrlId, [], productIds).pipe(
      takeUntil(this.destroy$),
      finalize(() => {
        for (const productId of productIds) {
          this.productRelationUpdatingIds.delete(productId);
        }
        this.cdr.markForCheck();
      }),
    ).subscribe({
      next: () => {
        if (this.selectedGameUrl?.id !== gameUrlId) {
          return;
        }

        for (const productId of productIds) {
          this.removedProductIds.add(productId);
          this.selectedProductIds.delete(productId);
        }
        this.loadFilteredProducts();
        this.cdr.markForCheck();
      },
      error: (error) => {
        if (this.selectedGameUrl?.id === gameUrlId) {
          this.productRelationError = this.getRequestError(
            error,
            'Unable to remove the selected products from the Game URL.',
          );
          this.cdr.markForCheck();
        }
      },
    });
  }

  setProductSelected(productId: number, selected: boolean, shiftKey = false): void {
    if (this.isProductRemoved(productId) || this.isProductRelationUpdating(productId)) {
      return;
    }

    this.clearProductPreview();
    const anchorIndex = this.productsFiltered.findIndex(product => product.productId === this.productSelectionAnchor);
    const endpointIndex = this.productsFiltered.findIndex(product => product.productId === productId);
    const hasRange = shiftKey && anchorIndex >= 0 && endpointIndex >= 0;
    const productIds = hasRange
      ? this.productsFiltered.slice(Math.min(anchorIndex, endpointIndex), Math.max(anchorIndex, endpointIndex) + 1)
          .map(product => product.productId)
      : [productId];

    for (const id of productIds) {
      if (selected && !this.isProductRemoved(id) && !this.isProductRelationUpdating(id)) {
        this.selectedProductIds.add(id);
      } else {
        this.selectedProductIds.delete(id);
      }
    }
    if (!hasRange) {
      this.productSelectionAnchor = productId;
    }
    this.cdr.markForCheck();
  }

  setFilteredProductsSelected(selected: boolean): void {
    this.clearProductPreview();
    this.productSelectionAnchor = null;
    for (const product of this.productsFiltered) {
      if (selected && !this.isProductRemoved(product.productId) && !this.isProductRelationUpdating(product.productId)) {
        this.selectedProductIds.add(product.productId);
      } else {
        this.selectedProductIds.delete(product.productId);
      }
    }
    this.cdr.markForCheck();
  }

  get canPauseAutomatedRun(): boolean {
    return this.automatedRun?.status === 'Queued' || this.automatedRun?.status === 'Running';
  }

  get canContinueAutomatedRun(): boolean {
    return this.automatedRun?.status === 'Paused';
  }

  get isAutomatedListingLimitValid(): boolean {
    return this.automatedListingLimitDraft !== null
      && Number.isInteger(this.automatedListingLimitDraft)
      && this.automatedListingLimitDraft > 0;
  }

  setAutomatedListingLimitDraft(value: number | null): void {
    this.automatedListingLimitDraft = value;
    this.automatedListingLimitDirty = value !== this.automatedRun?.setup.listingLimit;
    this.automatedCheckError = '';
  }

  applyAutomatedListingLimit(): void {
    const runId = this.automatedRunId;
    const listingLimit = this.automatedListingLimitDraft;
    if (
      runId === null ||
      !this.canContinueAutomatedRun ||
      !this.automatedListingLimitDirty ||
      !this.isAutomatedListingLimitValid ||
      this.automatedListingLimitPending
    ) {
      return;
    }

    this.automatedListingLimitPending = true;
    this.automatedCheckError = '';
    this.manualCheckService.updateListingLimit(runId, listingLimit!)
      .pipe(
        takeUntil(this.destroy$),
        finalize(() => {
          this.automatedListingLimitPending = false;
          this.cdr.markForCheck();
        }),
      )
      .subscribe({
        next: (run) => {
          this.automatedRun = run;
          this.automatedListingLimitDraft = run.setup.listingLimit;
          this.automatedListingLimitDirty = false;
          this.applyAutomatedRun(run);
          this.cdr.markForCheck();
        },
        error: (error) => {
          this.automatedCheckError = this.getRequestError(error, 'Unable to update the listings limit.');
          this.cdr.markForCheck();
        },
      });
  }

  pauseAutomatedRun(): void {
    const runId = this.automatedRunId;
    if (runId === null || !this.canPauseAutomatedRun || this.automatedPausePending) {
      return;
    }

    this.automatedPausePending = true;
    this.automatedCheckError = '';
    this.manualCheckService.pauseRun(runId)
      .pipe(
        takeUntil(this.destroy$),
        finalize(() => {
          this.automatedPausePending = false;
          this.cdr.markForCheck();
        }),
      )
      .subscribe({
        next: (run) => {
          this.automatedRun = run;
          this.applyAutomatedRun(run);
          this.cdr.markForCheck();
        },
        error: (error) => {
          this.automatedCheckError = this.getRequestError(error, 'Unable to pause the automated check.');
          this.cdr.markForCheck();
        },
      });
  }

  continueAutomatedRun(): void {
    const runId = this.automatedRunId;
    if (
      runId === null ||
      !this.canContinueAutomatedRun ||
      this.automatedContinuePending ||
      this.automatedListingLimitPending ||
      this.automatedListingLimitDirty ||
      !this.isAutomatedListingLimitValid
    ) {
      return;
    }

    this.automatedContinuePending = true;
    this.automatedCheckError = '';
    this.manualCheckService.continueRun(runId)
      .pipe(
        takeUntil(this.destroy$),
        finalize(() => {
          this.automatedContinuePending = false;
          this.cdr.markForCheck();
        }),
      )
      .subscribe({
        next: (accepted) => {
          if (this.automatedRun) {
            this.automatedRun = { ...this.automatedRun, ...accepted.run };
          }
          this.cdr.markForCheck();
        },
        error: (error) => {
          this.automatedCheckError = this.getRequestError(error, 'Unable to continue the automated check.');
          this.cdr.markForCheck();
        },
      });
  }

  cancelAutomatedRun(): void {
    const runId = this.automatedRunId;
    if (runId === null || !this.automatedRunActive || this.automatedCancelPending) {
      return;
    }

    this.automatedCancelPending = true;
    this.automatedCheckError = '';
    this.cdr.markForCheck();

    this.manualCheckService.cancelRun(runId)
      .pipe(
        takeUntil(this.destroy$),
        finalize(() => {
          this.automatedCancelPending = false;
          this.cdr.markForCheck();
        }),
      )
      .subscribe({
        next: (run) => {
          this.automatedRunStop$.next();
          this.automatedRun = run;
          this.applyAutomatedRun(run);
          this.cdr.markForCheck();
        },
        error: (error) => {
          this.automatedCheckError = this.getRequestError(error, 'Unable to cancel the automated check.');
          this.cdr.markForCheck();
        },
      });
  }

  openAllButtonClicked(): void {
    this.openTrustedUrls(
      this.productsFiltered.slice(0, 5).map((product) => product.fullUrl),
    );
  }

  startBatchButtonClicked(): void {
    const currentIndex = this.normalizedCurrentIndex();
    const batchSize = this.normalizedBatchSize();
    const toItem = currentIndex + batchSize;

    this.currentIndex = currentIndex;
    this.batchSize = batchSize;

    const selectedGameUrl = this.selectedGameUrl;
    if (this.isBatchMode(selectedGameUrl)) {
      for (let itemIndex = currentIndex; itemIndex < toItem; itemIndex++) {
        const url = (selectedGameUrl?.partialUrl ?? '').replace(
          '{0}',
          String(itemIndex),
        );

        const result = this.externalLinkDisclosure.openTrustedUrl(
          openableSteamUrl(url, this.openInSteamMode),
          '/manual-checks',
        );
        if (result === 'needs-disclosure') {
          break;
        }
      }
    } else {
      for (let itemIndex = currentIndex; itemIndex < toItem; itemIndex++) {
        const productIndex = itemIndex - 1;
        if (productIndex < 0 || productIndex >= this.productsFiltered.length) {
          break;
        }

        const result = this.externalLinkDisclosure.openTrustedUrl(
          openableSteamUrl(
            this.productsFiltered[productIndex].fullUrl,
            this.openInSteamMode,
          ),
          '/manual-checks',
        );
        if (result === 'needs-disclosure') {
          break;
        }
      }
    }

    this.cdr.markForCheck();
  }

  runPreviousBatchButtonClicked(): void {
    this.currentIndex = Math.max(
      1,
      this.normalizedCurrentIndex() - this.normalizedBatchSize(),
    );
    this.startBatchButtonClicked();
  }

  runNextBatchButtonClicked(): void {
    this.currentIndex =
      this.normalizedCurrentIndex() + this.normalizedBatchSize();
    this.startBatchButtonClicked();
  }

  clearBatchButtonClicked(): void {
    this.currentIndex = 1;
    this.batchSize = 1;
    this.cdr.markForCheck();
  }

  private normalizedCurrentIndex(): number {
    if (this.currentIndex === null || !Number.isFinite(this.currentIndex)) {
      return 1;
    }

    return Math.max(1, Math.trunc(this.currentIndex));
  }

  private normalizedBatchSize(): number {
    if (this.batchSize === null || !Number.isFinite(this.batchSize)) {
      return 1;
    }

    return Math.min(5, Math.max(1, Math.trunc(this.batchSize)));
  }

  getProductOpenUrl(url: string | null | undefined): string | null {
    return openableSteamUrl(url, this.openInSteamMode);
  }

  getProductOpenTooltip(url: string | null | undefined): string {
    const openUrl = this.getProductOpenUrl(url);
    if (!openUrl) {
      return 'Missing URL';
    }

    const warning = externalUrlWarning(openUrl);
    return warning ? `${warning} Destination: ${openUrl}` : openUrl;
  }

  clearFiltersButtonClicked(): void {
    this.searchByNameFilterControl.setValue('', { emitEvent: false });
    this.searchByRatingFilterControl.setValue(null, { emitEvent: false });
    this.searchByStockFilterControl.setValue(null, { emitEvent: false });
    this.stockStateFilterControl.setValue('all', { emitEvent: false });
    this.productSortControl.setValue('default', { emitEvent: false });
    this.automatedOutcomeFilter = 'All';

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

    const filters = new Set(this.tagsFilter);
    this.gameTagsFilter = this.gameTagsAll.filter(
      (tag) =>
        tag.gameId === this.gameIdControl.value &&
        (tag.name === null || !filters.has(tag.name.toLowerCase())),
    );

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

  private openTrustedUrls(urls: readonly (string | null | undefined)[]): void {
    let missing = false;

    for (const url of urls) {
      const result = this.externalLinkDisclosure.openTrustedUrl(
        openableSteamUrl(url, this.openInSteamMode),
        '/manual-checks',
      );

      if (result === 'blocked') {
        missing = true;
        continue;
      }

      if (result === 'needs-disclosure') {
        break;
      }
    }

    if (missing) {
      alert('Some links were not opened because they do not include a destination.');
    }
  }

  trackByProductId(_: number, product: GameUrlProduct): number {
    return product.productId;
  }

  isBatchMode(gameUrl: GameUrl | null): boolean {
    return (
      gameUrl?.scrapingModeId === ScrapingModeEnum.Batch ||
      gameUrl?.scrapingModeId === ScrapingModeEnum.PixelBatch
    );
  }

  private loadGames(): void {
    this.gameService
      .getAll()
      .pipe(takeUntil(this.destroy$))
      .subscribe((games) => {
        this.games = games.filter((game) => game.isActive);
        this.games$.next(this.games);
        this.cdr.markForCheck();
      });
  }

  private loadScrapingModes(): void {
    this.scrapingModeService
      .getAll()
      .pipe(takeUntil(this.destroy$))
      .subscribe((scrapingModes) => {
        this.scrapingModes$.next(
          [...scrapingModes]
            .filter((mode) => mode.id !== ScrapingModeEnum.PublicApi)
            .sort((a, b) => a.id - b.id),
        );
        this.cdr.markForCheck();
      });
  }

  private loadGameUrls(): void {
    this.gameUrlService
      .getAll()
      .pipe(takeUntil(this.destroy$))
      .subscribe((urls) => {
        this.gameUrlsAll = urls
          .filter((url) => url.isActive)
          .filter((url) => url.scrapingModeId !== ScrapingModeEnum.PublicApi);

        this.applySourceFilters(this.gameIdControl.value, this.scrapingModeIdControl.value);
      });
  }

  private loadGameTags(): void {
    this.tagsService
      .getAll()
      .pipe(takeUntil(this.destroy$))
      .subscribe((tags) => {
        this.gameTagsAll = tags.filter((tag) => tag.isActive);
        this.applySourceFilters(this.gameIdControl.value, this.scrapingModeIdControl.value);
      });
  }

  private applySourceFilters(gameId: number | null, scrapingModeId: number | null): void {
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
      this.resetProductRelationState();
      this.clearProductSelection();
      this.resetAutomatedCheck();

      this.cdr.markForCheck();
      return;
    }

    const urls = this.gameUrlsAll.filter((url) => {
      if (url.gameId !== gameId) {
        return false;
      }

      if (scrapingModeId !== null && url.scrapingModeId !== scrapingModeId) {
        return false;
      }

      return true;
    });
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
    this.resetProductRelationState();
    this.clearProductSelection();
    this.resetAutomatedCheck();

    this.cdr.markForCheck();
  }

  private loadFilteredProducts(): void {
    this.clearProductPreview();
    this.productSelectionAnchor = null;
    const nameFilter = (this.searchByNameFilterControl.value ?? '').toLowerCase();
    const tagFilters = this.tagsFilter.map((t) => t.toLowerCase());
    const ratingFilter = this.searchByRatingFilterControl.value;
    const exactStockFilter = this.searchByStockFilterControl.value;
    const stockStateFilter = this.stockStateFilterControl.value;

    const filtered = this.products.filter((product) => {
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

      const matchesAutomatedResult = this.automatedOutcomeFilter === 'All' ||
        this.getAutomatedProductOutcome(product.productId) === this.automatedOutcomeFilter;

      const matchesExactStock = exactStockFilter === null || product.currentStock === exactStockFilter;
      const matchesStockState = stockStateFilter === 'all' ||
        (stockStateFilter === 'negative' && product.currentStock < 0) ||
        (stockStateFilter === 'zero' && product.currentStock === 0) ||
        (stockStateFilter === 'positive' && product.currentStock > 0);

      return matchesName && matchesRating && matchesTags && matchesAutomatedResult &&
        matchesExactStock && matchesStockState;
    });

    const productSort = this.productSortControl.value;
    this.productsFiltered = productSort === 'default'
      ? filtered
      : [...filtered].sort((left, right) => {
          let comparison = 0;
          if (productSort === 'price-ascending' || productSort === 'price-descending') {
            const leftPrice = this.getAutomatedPriceTrace(left.productId)?.lowestCheckedPriceMinorUnits ?? null;
            const rightPrice = this.getAutomatedPriceTrace(right.productId)?.lowestCheckedPriceMinorUnits ?? null;

            if (leftPrice === null && rightPrice !== null) { return 1; }
            if (leftPrice !== null && rightPrice === null) { return -1; }
            if (leftPrice !== null && rightPrice !== null) {
              comparison = productSort === 'price-ascending'
                ? leftPrice - rightPrice
                : rightPrice - leftPrice;
            }
          } else if (productSort === 'stock-ascending' || productSort === 'stock-descending') {
            comparison = productSort === 'stock-ascending'
              ? left.currentStock - right.currentStock
              : right.currentStock - left.currentStock;
          } else {
            comparison = productSort === 'name-ascending'
              ? (left.productName ?? '').localeCompare(right.productName ?? '')
              : (right.productName ?? '').localeCompare(left.productName ?? '');
          }

          if (comparison !== 0) { return comparison; }

          const nameComparison = (left.productName ?? '').localeCompare(right.productName ?? '');
          return nameComparison !== 0 ? nameComparison : left.productId - right.productId;
        });
  }

  private updateProductStock(
    productId: number,
    operation: Observable<GameUrlProductCurrentStock>,
  ): void {
    this.stockUpdatingProductIds.add(productId);
    this.cdr.markForCheck();
    operation.pipe(
      takeUntil(this.destroy$),
      finalize(() => {
        this.stockUpdatingProductIds.delete(productId);
        this.cdr.markForCheck();
      }),
    ).subscribe({
      next: (result) => this.applyCurrentStock(productId, result.currentStock),
    });
  }

  private applyCurrentStock(productId: number, currentStock: number): void {
    const product = this.products.find((candidate) => candidate.productId === productId);
    if (!product) { return; }

    product.currentStock = currentStock;
    this.loadFilteredProducts();
    this.cdr.markForCheck();
  }

  private loadProductsGrid(gameUrlId: number): void {
    this.gameUrlProductService
      .existsByGameUrl(gameUrlId)
      .pipe(takeUntil(this.destroy$))
      .subscribe({
        next: (products) => {
          if (this.selectedGameUrl?.id !== gameUrlId) {
            return;
          }

          const activeProducts = products.filter((product) => product.isActive === true);
          const activeProductIds = new Set(activeProducts.map((product) => product.productId));
          const removedProducts = this.products.filter((product) =>
            product.gameUrlId === gameUrlId &&
            this.isProductRemoved(product.productId) &&
            !activeProductIds.has(product.productId));
          this.products = [...activeProducts, ...removedProducts];
          this.clearProductSelection();
          this.loadFilteredProducts();
          this.cdr.detectChanges();
        },
        error: (error) => {
          this.automatedCheckWarning = this.getRequestError(
            error,
            'The automated check started, but the product grid could not be loaded.',
          );
          this.cdr.markForCheck();
        },
      });
  }

  private startAutomatedRun(
    gameUrlId: number,
    presetId: number | null,
    presetCombination: ManualCheckPresetCombinationWrite | null,
    bypassCache: boolean,
    productIds: number[] | null,
  ): void {
    this.discardRemovedProducts();
    this.resetAutomatedCheck();
    if (this.products.length === 0) {
      this.loadProductsGrid(gameUrlId);
    }
    this.automatedRunActive = true;
    this.cdr.markForCheck();

    this.manualCheckService.createRun({ gameUrlId, presetId, presetCombination, bypassCache, productIds })
      .pipe(
        takeUntil(this.automatedRunStop$),
        takeUntil(this.destroy$),
      )
      .subscribe({
        next: (accepted) => this.pollAutomatedRun(accepted.runId),
        error: (error) => {
          this.automatedRunActive = false;
          this.automatedCheckError = this.getRequestError(error, 'Unable to start the automated check.');
          this.cdr.markForCheck();
        },
      });
  }

  private pollAutomatedRun(runId: number): void {
    this.resetAutomatedCheck();
    this.automatedRunId = runId;
    this.automatedRunActive = true;
    this.cdr.markForCheck();

    timer(0, 2000).pipe(
      switchMap(() => this.manualCheckService.getRun(runId)),
      takeUntil(this.automatedRunStop$),
      takeUntil(this.destroy$),
      takeWhile((run) => !this.isTerminalStatus(run), true),
      finalize(() => this.cdr.markForCheck()),
    ).subscribe({
      next: (run) => {
        this.automatedRun = run;
        this.applyAutomatedRun(run);
        this.cdr.markForCheck();
      },
      error: (error) => {
        this.automatedRunActive = false;
        this.automatedCheckError = this.getRequestError(error, 'Unable to retrieve automated-check progress.');
        this.cdr.markForCheck();
      },
    });
  }

  private applyAutomatedRun(run: ManualCheckRunDetail): void {
    const isTerminal = this.isTerminalStatus(run);
    this.automatedRunActive = !isTerminal;
    this.hasAutomatedCheckResult = isTerminal;
    if (!this.automatedListingLimitDirty || run.status !== 'Paused') {
      this.automatedListingLimitDraft = run.setup.listingLimit;
      this.automatedListingLimitDirty = false;
    }

    if (
      run.setup.products.length > 0 &&
      (this.products.length === 0 || this.products.some((product) => product.gameUrlId !== run.gameUrlId))
    ) {
      this.products = run.setup.products.map((product) => ({
        ...product,
        isActive: true,
        currentStock: 0,
      }));
      this.clearProductSelection();
    }

    this.automatedMatches.clear();
    this.automatedProductTraces.clear();
    this.automatedProductErrors.clear();
    this.automatedTargetProductIds.clear();

    for (const product of run.setup.products) {
      this.automatedTargetProductIds.add(product.productId);
    }

    for (const match of run.results.matches) {
      this.automatedMatches.set(match.productId, match);
    }

    for (const trace of run.results.productTraces) {
      this.automatedProductTraces.set(trace.productId, trace);
    }

    for (const error of run.results.errors) {
      this.automatedProductErrors.set(error.productId, error);
    }

    this.loadFilteredProducts();

    if (!isTerminal) {
      return;
    }

    if (run.status === 'CompletedWithErrors') {
      this.automatedCheckWarning = run.errorText
        || `${run.failedProducts} product page(s) failed. Partial matches are shown.`;
    } else if (run.status === 'Failed') {
      this.automatedCheckError = run.errorText || 'The automated check failed.';
    } else if (run.status === 'Canceled') {
      this.automatedCheckWarning = run.errorText || 'The automated check was canceled.';
    }
  }

  private resetAutomatedCheck(): void {
    this.automatedRunStop$.next();
    this.automatedRun = null;
    this.automatedRunId = null;
    this.automatedRunActive = false;
    this.automatedCancelPending = false;
    this.automatedPausePending = false;
    this.automatedContinuePending = false;
    this.automatedListingLimitDraft = null;
    this.automatedListingLimitDirty = false;
    this.automatedListingLimitPending = false;
    this.hasAutomatedCheckResult = false;
    this.automatedOutcomeFilter = 'All';
    this.automatedCheckWarning = '';
    this.automatedCheckError = '';
    this.automatedMatches.clear();
    this.automatedProductTraces.clear();
    this.automatedProductErrors.clear();
    this.automatedTargetProductIds.clear();
  }

  private isTerminalStatus(run: ManualCheckRunDetail): boolean {
    return run.status === 'Succeeded'
      || run.status === 'CompletedWithErrors'
      || run.status === 'Failed'
      || run.status === 'Canceled';
  }

  private clearProductSelection(): void {
    this.clearProductPreview();
    this.selectedProductIds.clear();
    this.productSelectionAnchor = null;
  }

  private discardRemovedProducts(): void {
    if (this.removedProductIds.size === 0) {
      return;
    }

    this.products = this.products.filter((product) => !this.removedProductIds.has(product.productId));
    for (const productId of this.removedProductIds) {
      this.selectedProductIds.delete(productId);
    }
    this.removedProductIds.clear();
    this.productRelationError = '';
    this.loadFilteredProducts();
  }

  private resetProductRelationState(): void {
    this.removedProductIds.clear();
    this.productRelationError = '';
  }

  private getRequestError(error: unknown, fallback: string): string {
    if (error instanceof HttpErrorResponse) {
      return error.error?.detail ?? error.error?.message ?? fallback;
    }
    return error instanceof Error ? error.message : fallback;
  }
}
