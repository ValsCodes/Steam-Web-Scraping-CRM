import {
  ChangeDetectionStrategy,
  ChangeDetectorRef,
  Component,
  OnDestroy,
  OnInit,
} from '@angular/core';
import { CommonModule } from '@angular/common';
import { HttpErrorResponse } from '@angular/common/http';
import { FormControl, ReactiveFormsModule } from '@angular/forms';
import {
  BehaviorSubject,
  finalize,
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
  ManualCheckProductError,
  ManualCheckProductResult,
  ManualCheckProductTrace,
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
import { groupByItemGroup, ItemGroupSection } from '../../common/item-grouping';

@Component({
  selector: 'steam-manual-mode-v2',
  standalone: true,
  changeDetection: ChangeDetectionStrategy.OnPush,
  imports: [
    CommonModule,
    ReactiveFormsModule,
    MatTooltip,
    MatDialogModule,
    MatButtonModule,
    MatIconModule,
    MatMenuModule,
    ExternalLinkDirective
],
  templateUrl: './manual-mode-v2.html',
  styleUrl: './manual-mode-v2.scss',
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
  hasAutomatedCheckResult = false;
  showAutomatedMatchesOnly = false;
  automatedCheckWarning = '';
  automatedCheckError = '';
  readonly selectedProductIds = new Set<number>();

  readonly gameIdControl = new FormControl<number | null>(null);
  readonly scrapingModeIdControl = new FormControl<number | null>(null);
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
  private readonly automatedRunStop$ = new Subject<void>();

  private games: Game[] = [];
  private gameUrlsAll: GameUrl[] = [];
  private gameTagsAll: Tag[] = [];
  private readonly automatedMatches = new Map<number, ManualCheckProductResult>();
  private readonly automatedProductTraces = new Map<number, ManualCheckProductTrace>();
  private readonly automatedProductErrors = new Map<number, ManualCheckProductError>();
  private readonly automatedTargetProductIds = new Set<number>();
  private readonly lastPresetByGame = new Map<number, number>();

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
          this.clearProductSelection();
          this.resetAutomatedCheck();
          this.cdr.markForCheck();
          return;
        }

        this.selectedGameUrl =
          this.gameUrlsFiltered$.value.find((u) => u.id === gameUrlId) ?? null;

        this.clearBatchButtonClicked();
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
      gameName,
      gameUrlName: source.name ?? `Game URL #${source.id}`,
      preselectedPresetId: this.lastPresetByGame.get(gameId),
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
      if (result === undefined) {
        return;
      }
      this.lastPresetByGame.set(gameId, result.presetId);
      this.startAutomatedRun(
        source.id,
        result.presetId,
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

  setAutomatedMatchesOnly(enabled: boolean): void {
    this.showAutomatedMatchesOnly = enabled;
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
    if (!this.automatedTargetProductIds.has(productId)) {
      return null;
    }

    if (this.automatedProductErrors.has(productId)) {
      return 'Failed';
    }

    if (this.automatedMatches.has(productId)) {
      return 'Matched';
    }

    const trace = this.automatedProductTraces.get(productId);
    return trace?.matchEvaluated ? 'No match' : 'Pending';
  }

  get selectedProductCount(): number {
    return this.selectedProductIds.size;
  }

  get areAllFilteredProductsSelected(): boolean {
    return this.productsFiltered.length > 0 &&
      this.productsFiltered.every((product) => this.selectedProductIds.has(product.productId));
  }

  get areSomeFilteredProductsSelected(): boolean {
    const selectedCount = this.productsFiltered.filter(
      (product) => this.selectedProductIds.has(product.productId),
    ).length;
    return selectedCount > 0 && selectedCount < this.productsFiltered.length;
  }

  isProductSelected(productId: number): boolean {
    return this.selectedProductIds.has(productId);
  }

  setProductSelected(productId: number, selected: boolean): void {
    if (selected) {
      this.selectedProductIds.add(productId);
    } else {
      this.selectedProductIds.delete(productId);
    }
    this.cdr.markForCheck();
  }

  setFilteredProductsSelected(selected: boolean): void {
    for (const product of this.productsFiltered) {
      if (selected) {
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
    if (runId === null || !this.canContinueAutomatedRun || this.automatedContinuePending) {
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
          '/manual-mode-v2',
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
          '/manual-mode-v2',
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
    this.showAutomatedMatchesOnly = false;

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
        '/manual-mode-v2',
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
    this.clearProductSelection();
    this.resetAutomatedCheck();

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

      const matchesAutomatedResult =
        !this.showAutomatedMatchesOnly || this.automatedMatches.has(product.productId);

      return matchesName && matchesRating && matchesTags && matchesAutomatedResult;
    });
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

          this.products = products.filter((product) => product.isActive === true);
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
    presetId: number,
    bypassCache: boolean,
    productIds: number[] | null,
  ): void {
    this.resetAutomatedCheck();
    if (this.products.length === 0) {
      this.loadProductsGrid(gameUrlId);
    }
    this.automatedRunActive = true;
    this.cdr.markForCheck();

    this.manualCheckService.createRun({ gameUrlId, presetId, bypassCache, productIds })
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

    if (
      run.setup.products.length > 0 &&
      (this.products.length === 0 || this.products.some((product) => product.gameUrlId !== run.gameUrlId))
    ) {
      this.products = run.setup.products.map((product) => ({
        ...product,
        isActive: true,
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
    this.hasAutomatedCheckResult = false;
    this.showAutomatedMatchesOnly = false;
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
    this.selectedProductIds.clear();
  }

  private getRequestError(error: unknown, fallback: string): string {
    if (error instanceof HttpErrorResponse) {
      return error.error?.detail ?? error.error?.message ?? fallback;
    }
    return error instanceof Error ? error.message : fallback;
  }
}
