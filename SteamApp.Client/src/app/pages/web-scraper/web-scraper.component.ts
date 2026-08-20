import {
  ChangeDetectorRef,
  Component,
  DestroyRef,
  ViewChild,
  computed,
  effect,
  inject,
  signal,
} from '@angular/core';
import { FormControl, FormsModule, ReactiveFormsModule } from '@angular/forms';
import { CommonModule } from '@angular/common';
import { MatTableDataSource, MatTableModule } from '@angular/material/table';
import { MatPaginator, MatPaginatorModule } from '@angular/material/paginator';
import { MatSort, MatSortModule } from '@angular/material/sort';
import { MatDialog, MatDialogModule } from '@angular/material/dialog';
import { toSignal, takeUntilDestroyed } from '@angular/core/rxjs-interop';
import {
  startWith,
  map,
  finalize,
  Subject,
  takeUntil,
  Observable,
  switchMap,
  takeWhile,
  timer,
  tap,
} from 'rxjs';
import * as XLSX from 'xlsx';

import { SteamService } from '../../services/steam/steam.service';
import { StopwatchComponent } from '../../components';
import {
  Game,
  GameUrl,
  ScrapeHistoryDetail,
  ScrapeJobAccepted,
  ScrapeJobStatus,
  ScrapingMode as ScrapingModeLookup,
  ScrapingModeEnum,
} from '../../models';
import { Listing } from '../../models/listing.model';
import { GameService, GameUrlService, ScrapingModeService } from '../../services';
import { MatTooltip } from '@angular/material/tooltip';
import {
  ExternalLinkDirective,
  externalUrlWarning,
  getListingUrl,
  openableExternalUrl,
  openableSteamUrl,
  safeExternalImageUrl,
} from '../../common';
import { ScrapeHistoryDialogComponent } from './scrape-history-dialog.component';

enum ScraperExecutionMode {
  WebScrape = ScrapingModeEnum.Batch,
  PixelScrape = ScrapingModeEnum.PixelBatch,
  PublicApi = ScrapingModeEnum.PublicApi,
}

type ScraperExecutionModeItem = {
  id: ScraperExecutionMode;
  name: string;
};

@Component({
  selector: 'web-scraper',
  standalone: true,
  imports: [
    CommonModule,
    FormsModule,
    ReactiveFormsModule,
    MatTableModule,
    MatPaginatorModule,
    MatSortModule,
    MatDialogModule,
    StopwatchComponent,
    MatTooltip,
    ExternalLinkDirective,
  ],
  templateUrl: './web-scraper.component.html',
  styleUrl: './web-scraper.component.scss',
  providers: [SteamService],
})
export class WebScraperComponent {
  readonly safeExternalImageUrl = safeExternalImageUrl;
  readonly externalUrlWarning = externalUrlWarning;

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

  @ViewChild(StopwatchComponent) stopwatch?: StopwatchComponent;

  readonly ScraperExecutionMode = ScraperExecutionMode;

  private readonly destroyRef = inject(DestroyRef);
  private cancel$ = new Subject<void>();

  readonly gameIdControl = new FormControl<number | null>(null);
  readonly scrapingModeIdControl = new FormControl<number | null>(null);
  readonly gameUrlIdControl = new FormControl<number | null>(null);

  readonly selectedGameId = toSignal(
    this.gameIdControl.valueChanges.pipe(startWith(this.gameIdControl.value)),
    { initialValue: null },
  );

  readonly selectedGameUrlId = toSignal(
    this.gameUrlIdControl.valueChanges.pipe(
      startWith(this.gameUrlIdControl.value),
    ),
    { initialValue: null },
  );

  readonly selectedScrapingModeId = toSignal(
    this.scrapingModeIdControl.valueChanges.pipe(
      startWith(this.scrapingModeIdControl.value),
    ),
    { initialValue: null },
  );

  readonly games = toSignal(
    this.gameService
      .getAll()
      .pipe(map((games) => games.filter((game) => game.isActive))),
    {
      initialValue: [] as Game[],
    },
  );

  readonly scrapingModes = toSignal(
    this.scrapingModeService
      .getAll()
      .pipe(
        map((modes) =>
          [...modes]
            .filter((mode) => mode.id !== 1)
            .sort((a, b) => a.id - b.id),
        ),
      ),
    { initialValue: [] as ScrapingModeLookup[] },
  );

  readonly selectedGame = computed<Game | null>(() => {
    const urlId = this.selectedGameId();
    if (urlId === null) {
      return null;
    }
    return this.games().find((u) => u.id === urlId) ?? null;
  });

  readonly gameUrlsAll = toSignal(
    this.gameUrlService.getAll().pipe(
      map((urls): GameUrl[] =>
        urls
          .filter((url) => url.isActive)
          .filter((url) => {
            const scrapingModeId = url.scrapingModeId ?? null;
            return (
              scrapingModeId !== null &&
              scrapingModeId !== ScrapingModeEnum.ManualBatch
            );
          }),
      ),
    ),
    { initialValue: [] as GameUrl[] },
  );

  readonly gameUrlsFiltered = computed(() => {
    const gameId = this.selectedGameId();
    const scrapingModeId = this.selectedScrapingModeId();
    if (gameId === null) {
      return [];
    }
    return this.gameUrlsAll().filter((u) => {
      if (u.gameId !== gameId) {
        return false;
      }

      if (scrapingModeId !== null && u.scrapingModeId !== scrapingModeId) {
        return false;
      }

      return true;
    });
  });

  readonly selectedGameUrl = computed<GameUrl | null>(() => {
    const urlId = this.selectedGameUrlId();
    if (urlId === null) {
      return null;
    }
    return this.gameUrlsFiltered().find((u) => u.id === urlId) ?? null;
  });

  readonly availableModes = computed<ScraperExecutionModeItem[]>(() => {
    const url = this.selectedGameUrl();
    if (!url) {
      return [];
    }

    return this.getExecutionModesForGameUrl(url);
  });

  readonly selectedMode = signal<ScraperExecutionModeItem | null>(null);
  readonly statusLabel = signal<string>('');
  readonly isLoading = signal<boolean>(false);
  readonly pageNumber = signal<number>(1);
  readonly openInSteamMode = signal<boolean>(false);

  readonly canRun = computed(() => {
    return (
      this.selectedGameUrl() !== null &&
      this.selectedMode() !== null &&
      this.pageNumber() > 0 &&
      !this.isLoading()
    );
  });

  dataSource = new MatTableDataSource<Listing>([]);
  displayedColumns: string[] = [
    'rowNumber',
    'name',
    'quantity',
    'color',
    'price',
    'actions',
  ];

  constructor(
    private readonly steamService: SteamService,
    private readonly cdr: ChangeDetectorRef,
    private readonly gameService: GameService,
    private readonly gameUrlService: GameUrlService,
    private readonly scrapingModeService: ScrapingModeService,
    private readonly dialog: MatDialog,
  ) {
    effect(() => {
      void this.selectedGameId();
      this.gameUrlIdControl.setValue(null, { emitEvent: true });
      this.selectedMode.set(null);
      this.pageNumber.set(1);
      this.dataSource.data = [];
      this.statusLabel.set('');
      this.cdr.markForCheck();
    });

    effect(() => {
      void this.selectedScrapingModeId();
      this.gameUrlIdControl.setValue(null, { emitEvent: true });
      this.selectedMode.set(null);
      this.pageNumber.set(1);
      this.dataSource.data = [];
      this.statusLabel.set('');
      this.cdr.markForCheck();
    });

    effect(() => {
      void this.selectedGameUrl();
      this.selectedMode.set(null);
      this.pageNumber.set(1);
      this.cancelActiveRequest();
      this.dataSource.data = [];
      this.statusLabel.set('');
      this.cdr.markForCheck();
    });
  }

  private requireGameUrlId(): number {
    const id = this.gameUrlIdControl.value;
    if (id === null || id <= 0) {
      throw new Error('Game URL is required.');
    }
    return id;
  }

  private getExecutionModesForGameUrl(
    url: GameUrl,
  ): ScraperExecutionModeItem[] {
    switch (url.scrapingModeId) {
      case ScraperExecutionMode.WebScrape:
        return [{ id: ScraperExecutionMode.WebScrape, name: 'Web Scrape' }];
      case ScraperExecutionMode.PixelScrape:
        return [
          { id: ScraperExecutionMode.WebScrape, name: 'Web Scrape' },
          { id: ScraperExecutionMode.PixelScrape, name: 'Pixel Scrape' },
        ];
      case ScraperExecutionMode.PublicApi:
        return [
          { id: ScraperExecutionMode.PublicApi, name: 'Public API Scrape' },
        ];
      default:
        return [];
    }
  }

  public setSelectedMode(modeId: ScraperExecutionMode): void {
    const mode = this.availableModes().find((m) => m.id === modeId) ?? null;
    this.selectedMode.set(mode);

    if (mode === null) {
      this.statusLabel.set('Invalid mode selected.');
      return;
    }

    this.statusLabel.set(`Mode selected: ${mode.name}.`);
  }

  public isModeSelected(modeId: ScraperExecutionMode): boolean {
    return this.selectedMode()?.id === modeId;
  }

  private setLoading(): void {
    this.dataSource.data = [];
    this.isLoading.set(true);
    this.statusLabel.set('Loading...');
    this.stopwatch?.start();
  }

  private finishLoading(): void {
    this.isLoading.set(false);
    this.stopwatch?.stop();
  }

  private cancelActiveRequest(): void {
    this.cancel$.next();
    this.cancel$ = new Subject<void>();
    this.isLoading.set(false);
  }

  private queueRequestForMode(mode: ScraperExecutionMode): Observable<ScrapeJobAccepted> {
    const gameUrlId = this.requireGameUrlId();
    const page = this.pageNumber();

    switch (mode) {
      case ScraperExecutionMode.WebScrape: {
        return this.steamService.queueScrapePage(gameUrlId, page);
      }
      case ScraperExecutionMode.PublicApi: {
        return this.steamService.queueScrapeFromPublicApi(gameUrlId, page);
      }
      case ScraperExecutionMode.PixelScrape: {
        return this.steamService.queueScrapeForPixels(gameUrlId, page);
      }
      default: {
        throw new Error('Unsupported scraping mode.');
      }
    }
  }

  private pollScrapeJob(historyId: number): Observable<ScrapeHistoryDetail> {
    return timer(0, 2000).pipe(
      switchMap(() => this.steamService.getScrapeHistoryDetail(historyId)),
      takeWhile((detail) => !this.isTerminalStatus(this.getStatus(detail)), true),
    );
  }

  private applyScrapeJobDetail(
    detail: ScrapeHistoryDetail,
    successMessage: string,
  ): void {
    const status = this.getStatus(detail);

    if (status === 'Succeeded') {
      this.dataSource.data = this.parseListings(detail.resultsJson);
      this.attachTableControls();
      this.statusLabel.set(successMessage);
      this.cdr.markForCheck();
      return;
    }

    if (status === 'Failed') {
      this.dataSource.data = [];
      this.statusLabel.set(
        detail.errorText ?? `Failed to run ${detail.scrapeType}.`,
      );
      this.cdr.markForCheck();
      return;
    }

    this.statusLabel.set(`${detail.scrapeType} ${status.toLowerCase()}...`);
    this.cdr.markForCheck();
  }

  private parseListings(resultsJson: string | null | undefined): Listing[] {
    if (!resultsJson) {
      return [];
    }

    try {
      const parsed = JSON.parse(resultsJson);
      const rows = Array.isArray(parsed)
        ? parsed
        : this.isRecord(parsed) && Array.isArray(parsed['results'])
          ? parsed['results']
          : [];

      return rows
        .map((row) => this.normalizeListing(row))
        .filter((row): row is Listing => row !== null);
    } catch {
      return [];
    }
  }

  private normalizeListing(value: unknown): Listing | null {
    if (!this.isRecord(value)) {
      return null;
    }

    const isPainted = this.readBoolean(value, 'isPainted', 'IsPainted') ?? false;
    const directPrice = this.readNumber(value, 'price', 'Price');
    const sellPriceInCents = this.readNumber(value, 'sellPrice', 'SellPrice', 'sell_price');
    const iconUrl = this.readString(value, 'iconUrl', 'IconUrl', 'icon_url')
      ?? this.readNestedString(value, ['assetDescription', 'iconUrl'], ['AssetDescription', 'IconUrl'], ['asset_description', 'icon_url']);

    return {
      name: this.readString(value, 'name', 'Name', 'marketName', 'MarketName', 'market_name') ?? '',
      price: directPrice ?? (sellPriceInCents !== null ? sellPriceInCents / 100 : 0),
      imageUrl: this.readString(value, 'imageUrl', 'ImageUrl', 'appIcon', 'AppIcon', 'app_icon')
        ?? (iconUrl ? `https://community.akamai.steamstatic.com/economy/image/${iconUrl}/62fx62f` : ''),
      quantity: this.readNumber(value, 'quantity', 'Quantity', 'sellListings', 'SellListings', 'sell_listings') ?? 0,
      pixelName: this.readString(value, 'pixelName', 'PixelName') ?? '',
      linkUrl: this.readString(value, 'linkUrl', 'LinkUrl', 'listingUrl', 'ListingUrl') ?? '',
      pageUrl: this.readString(value, 'pageUrl', 'PageUrl') ?? '',
      redValue: isPainted ? this.readNumber(value, 'redValue', 'RedValue') : null,
      greenValue: isPainted ? this.readNumber(value, 'greenValue', 'GreenValue') : null,
      blueValue: isPainted ? this.readNumber(value, 'blueValue', 'BlueValue') : null,
      isPainted,
    };
  }

  private isRecord(value: unknown): value is Record<string, unknown> {
    return typeof value === 'object' && value !== null;
  }

  private readString(
    source: Record<string, unknown>,
    ...keys: string[]
  ): string | null {
    for (const key of keys) {
      const value = source[key];
      if (typeof value === 'string' && value.trim()) {
        return value;
      }
    }

    return null;
  }

  private readNumber(
    source: Record<string, unknown>,
    ...keys: string[]
  ): number | null {
    for (const key of keys) {
      const value = source[key];
      if (typeof value === 'number' && Number.isFinite(value)) {
        return value;
      }

      if (typeof value === 'string' && value.trim()) {
        const parsed = Number(value);
        if (Number.isFinite(parsed)) {
          return parsed;
        }
      }
    }

    return null;
  }

  private readBoolean(
    source: Record<string, unknown>,
    ...keys: string[]
  ): boolean | null {
    for (const key of keys) {
      const value = source[key];
      if (typeof value === 'boolean') {
        return value;
      }
    }

    return null;
  }

  private readNestedString(
    source: Record<string, unknown>,
    ...paths: string[][]
  ): string | null {
    for (const path of paths) {
      let current: unknown = source;

      for (const segment of path) {
        if (!this.isRecord(current)) {
          current = null;
          break;
        }

        current = current[segment];
      }

      if (typeof current === 'string' && current.trim()) {
        return current;
      }
    }

    return null;
  }

  private getStatus(history: { status?: ScrapeJobStatus | null; isHaveError?: boolean }): ScrapeJobStatus {
    return history.status ?? (history.isHaveError ? 'Failed' : 'Succeeded');
  }

  private isTerminalStatus(status: ScrapeJobStatus): boolean {
    return status === 'Succeeded' || status === 'Failed';
  }

  private getErrorMessage(err: any, fallback: string): string {
    return typeof err?.error === 'string'
      ? err.error
      : err?.error?.message ?? err?.message ?? fallback;
  }

  runButtonClicked(): void {
    const selectedUrl = this.selectedGameUrl();
    const selectedMode = this.selectedMode();

    if (selectedUrl === null) {
      this.statusLabel.set('Select a game URL.');
      return;
    }

    if (selectedMode === null) {
      this.statusLabel.set('Select a mode.');
      return;
    }

    if (this.pageNumber() < 1) {
      this.statusLabel.set('Enter a valid page number.');
      return;
    }

    this.setLoading();

    this.queueRequestForMode(selectedMode.id)
      .pipe(
        tap((response) => {
          this.statusLabel.set(
            `Queued ${selectedMode.name} on page ${this.pageNumber()}.`,
          );
          this.cdr.markForCheck();
        }),
        switchMap((response) => this.pollScrapeJob(response.historyId)),
        takeUntil(this.cancel$),
        takeUntilDestroyed(this.destroyRef),
        finalize(() => this.finishLoading()),
      )
      .subscribe({
        next: (detail) => {
          this.applyScrapeJobDetail(
            detail,
            `Successfully ran ${selectedMode.name} on page ${this.pageNumber()}.`,
          );
        },
        error: (err: any) => {
          this.statusLabel.set(
            this.getErrorMessage(err, `Failed to run ${selectedMode.name}.`),
          );
        },
      });
  }

  onPageNumberChange(value: string | number): void {
    const parsed = Number(value);

    if (!Number.isInteger(parsed) || parsed < 1 || parsed > 100000) {
      this.statusLabel.set('Enter a valid page number.');
      return;
    }

    this.pageNumber.set(parsed);
    this.statusLabel.set(`Page set to ${parsed}.`);
  }

  nextPageButtonClicked(): void {
    this.pageNumber.set(this.pageNumber() + 1);
    this.statusLabel.set(`Page set to ${this.pageNumber()}.`);
  }

  previousPageButtonClicked(): void {
    if (this.pageNumber() <= 1) {
      return;
    }

    this.pageNumber.set(this.pageNumber() - 1);
    this.statusLabel.set(`Page set to ${this.pageNumber()}.`);
  }

  cancelAll(): void {
    this.cancelActiveRequest();
    this.stopwatch?.stop();
    this.statusLabel.set('Operation cancelled.');
  }

  clearButtonClicked(): void {
    this.cancelActiveRequest();
    this.stopwatch?.reset();
    this.dataSource.data = [];
    this.statusLabel.set('');
    this.gameIdControl.setValue(null);
    this.scrapingModeIdControl.setValue(null);
    this.gameUrlIdControl.setValue(null);
    this.selectedMode.set(null);
    this.pageNumber.set(1);
  }

  historyButtonClicked(): void {
    this.dialog
      .open<
        ScrapeHistoryDialogComponent,
        unknown,
        ScrapeJobAccepted | undefined
      >(ScrapeHistoryDialogComponent, {
        maxWidth: '96vw',
        maxHeight: '90vh',
      })
      .afterClosed()
      .pipe(takeUntilDestroyed(this.destroyRef))
      .subscribe((response) => {
        if (!response) {
          return;
        }

        this.setLoading();
        this.statusLabel.set(
          `Queued rerun for ${response.history.scrapeType} on page ${response.history.page}.`,
        );

        this.pollScrapeJob(response.historyId)
          .pipe(
            takeUntil(this.cancel$),
            takeUntilDestroyed(this.destroyRef),
            finalize(() => this.finishLoading()),
          )
          .subscribe({
            next: (detail) => {
              this.applyScrapeJobDetail(
                detail,
                `Reran ${response.history.scrapeType} from history on page ${response.history.page}.`,
              );
            },
            error: (err: any) => {
              this.statusLabel.set(
                this.getErrorMessage(err, 'Failed to rerun scrape history.'),
              );
            },
          });
      });
  }

  exportButtonClicked(): void {
    const worksheet: XLSX.WorkSheet = XLSX.utils.json_to_sheet(
      this.dataSource.data,
    );
    const workbook: XLSX.WorkBook = XLSX.utils.book_new();

    XLSX.utils.book_append_sheet(workbook, worksheet, 'Data');

    const today = new Date();
    XLSX.writeFile(workbook, `Export_${today.toDateString()}_Listings.xlsx`);
  }

  getShowPageUrl(): string {
    return (
      this.selectedGameUrl()?.partialUrl?.replace(
        '{0}',
        this.pageNumber().toString(),
      ) ?? ''
    );
  }

  getSafeShowPageUrl(): string | null {
    return openableSteamUrl(this.getShowPageUrl(), this.openInSteamMode());
  }

  getSafeListingUrl(listingName: string | null | undefined): string | null {
    if (
      this.selectedGame() === null ||
      this.selectedGame()?.internalId === null ||
      this.selectedGame()?.internalId! <= 0
    ) {
      return null;
    }

    const name = listingName?.trim();

    if (!name) {
      return null;
    }

    const url = getListingUrl(
      this.selectedGame()?.internalId,
      name,
    );
    if (url === '') {
      return null;
    }

    return openableExternalUrl(url);
  }

  private attachTableControls(): void {
    this.dataSource.paginator = this.paginator ?? null;
    this.dataSource.sort = this.sort ?? null;

    if (!this.paginator) {
      return;
    }

    this.paginator.pageSize = 10;
    this.paginator.pageSizeOptions = [10, 25, 50];
  }
}
