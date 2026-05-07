import {
  AfterViewInit,
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
import { toSignal, takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { startWith, map, finalize, Subject, takeUntil, Observable } from 'rxjs';
import * as XLSX from 'xlsx';

import { SteamService } from '../../services/steam/steam.service';
import { StopwatchComponent } from '../../components';
import {
  Game,
  GameUrl,
  ScrapingMode as ScrapingModeLookup,
  ScrapingModeEnum,
} from '../../models';
import { Listing } from '../../models/listing.model';
import { GameService, GameUrlService, ScrapingModeService } from '../../services';
import { MatTooltip } from '@angular/material/tooltip';

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
    StopwatchComponent,
    MatTooltip,
  ],
  templateUrl: './web-scraper.component.html',
  styleUrl: './web-scraper.component.scss',
  providers: [SteamService],
})
export class WebScraperComponent implements AfterViewInit {
  @ViewChild(MatPaginator) paginator!: MatPaginator;
  @ViewChild(MatSort) sort!: MatSort;
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
    this.gameService.getAll().pipe(
      map((games) => games.filter((game) => game.isActive)),
    ),
    {
    initialValue: [] as Game[],
    },
  );

  readonly scrapingModes = toSignal(
    this.scrapingModeService.getAll().pipe(
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
            return scrapingModeId !== null && scrapingModeId !== ScrapingModeEnum.ManualBatch;
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
  ) {
    effect(
      () => {
        void this.selectedGameId();
        this.gameUrlIdControl.setValue(null, { emitEvent: true });
        this.selectedMode.set(null);
        this.pageNumber.set(1);
        this.dataSource.data = [];
        this.statusLabel.set('');
        this.cdr.markForCheck();
      },
      { allowSignalWrites: true },
    );

    effect(
      () => {
        void this.selectedScrapingModeId();
        this.gameUrlIdControl.setValue(null, { emitEvent: true });
        this.selectedMode.set(null);
        this.pageNumber.set(1);
        this.dataSource.data = [];
        this.statusLabel.set('');
        this.cdr.markForCheck();
      },
      { allowSignalWrites: true },
    );

    effect(
      () => {
        void this.selectedGameUrl();
        this.selectedMode.set(null);
        this.pageNumber.set(1);
        this.cancelActiveRequest();
        this.dataSource.data = [];
        this.statusLabel.set('');
        this.cdr.markForCheck();
      },
      { allowSignalWrites: true },
    );
  }

  ngAfterViewInit(): void {
    this.dataSource.paginator = this.paginator;
    this.dataSource.paginator.pageSize = 10;
    this.dataSource.paginator.pageSizeOptions = [10, 25, 50];
    this.dataSource.sort = this.sort;
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
        return [{ id: ScraperExecutionMode.PublicApi, name: 'Public API Scrape' }];
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

  private getRequestForMode(mode: ScraperExecutionMode): Observable<Listing[]> {
    const gameUrlId = this.requireGameUrlId();
    const page = this.pageNumber();

    switch (mode) {
      case ScraperExecutionMode.WebScrape: {
        return this.steamService.scrapePage(gameUrlId, page);
      }
      case ScraperExecutionMode.PublicApi: {
        return this.steamService.scrapeFromPublicApi(gameUrlId, page);
      }
      case ScraperExecutionMode.PixelScrape: {
        return this.steamService.scrapeForPixels(gameUrlId, page);
      }
      default: {
        throw new Error('Unsupported scraping mode.');
      }
    }
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

    this.getRequestForMode(selectedMode.id)
      .pipe(
        takeUntil(this.cancel$),
        takeUntilDestroyed(this.destroyRef),
        finalize(() => this.finishLoading()),
      )
      .subscribe({
        next: (response: Listing[]) => {
          this.dataSource.data = response ?? [];
          this.statusLabel.set(
            `Successfully ran ${selectedMode.name} on page ${this.pageNumber()}.`,
          );
        },
        error: (err: any) => {
          this.statusLabel.set(
            err?.error ?? err?.message ?? `Failed to run ${selectedMode.name}.`,
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

  getListingUrl(): string {
    if (
      this.selectedGame() === null ||
      this.selectedGame()?.internalId === null ||
      this.selectedGame()?.internalId! <= 0
    ) {
      return '';
    }

    return (
      'https://steamcommunity.com/market/listings/{0}/'?.replace(
        '{0}',
        this.selectedGame()!.internalId!.toString(),
      ) ?? ''
    );
  }
}
