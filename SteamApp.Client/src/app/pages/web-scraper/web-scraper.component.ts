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
import { startWith, map, finalize, Subject, takeUntil } from 'rxjs';
import * as XLSX from 'xlsx';

import { SteamService } from '../../services/steam/steam.service';
import { StopwatchComponent } from '../../components';
import { Game, GameUrl } from '../../models';
import { Listing } from '../../models/listing.model';
import { GameService, GameUrlService } from '../../services';

enum ScrapingMode {
  Scraper = 1,
  PublicApiScraper = 2,
  PixelScrape = 3,
}

type ScrapingModeItem = {
  id: ScrapingMode;
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
  ],
  templateUrl: './web-scraper.component.html',
  styleUrl: './web-scraper.component.scss',
  providers: [SteamService],
})
export class WebScraperComponent implements AfterViewInit {
  @ViewChild(MatPaginator) paginator!: MatPaginator;
  @ViewChild(MatSort) sort!: MatSort;

  private readonly destroyRef = inject(DestroyRef);
  private cancel$ = new Subject<void>();

  readonly gameIdControl = new FormControl<number | null>(null);
  readonly gameUrlIdControl = new FormControl<number | null>(null);

  // FormControl values as signals
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

  // Data from services as signals
  readonly games = toSignal(this.gameService.getAll(), {
    initialValue: [] as Game[],
  });

readonly gameUrlsAll = toSignal(
  this.gameUrlService.getAll().pipe(
    map((urls): GameUrl[] =>
      urls
        .filter((x) => x.isBatchUrl || x.isPixelScrape || x.isPublicApi)
        .map((url): GameUrl => {
          const baseName = url.name ?? '';

          const suffix =
            (url.isBatchUrl ? ' [ Batch ]' : '') +
            (url.isPixelScrape ? ' [ Pixel ]' : '') +
            (url.isPublicApi ? ' [ Public API ]' : '');

          return {
            ...url,
            name: (baseName + suffix) as any, // remove this cast if GameUrl.name is string | null | undefined and you accept string
          };
        }),
    ),
  ),
  { initialValue: [] as GameUrl[] },
);

  readonly gameUrlsFiltered = computed(() => {
    const gameId = this.selectedGameId();
    if (gameId === null) {
      return [];
    }
    return this.gameUrlsAll()?.filter((u) => u.gameId === gameId);
  });

  readonly selectedGameUrl = computed<GameUrl | null>(() => {
    const urlId = this.selectedGameUrlId();
    if (urlId === null) {
      return null;
    }
    return this.gameUrlsFiltered()?.find((u) => u.id === urlId) ?? null;
  });

  readonly ScrapingModes: readonly ScrapingModeItem[] = [
    { id: ScrapingMode.Scraper, name: 'Web Scraper' },
    { id: ScrapingMode.PublicApiScraper, name: 'Public API Scrape' },
    { id: ScrapingMode.PixelScrape, name: 'Pixel Scrape' },
  ];

  readonly selectedMode = signal<ScrapingModeItem | null>(null);
  readonly statusLabel = signal<string>('');
  readonly isLoading = signal<boolean>(false);
  readonly pageNumber = signal<number>(1);

  dataSource = new MatTableDataSource<Listing>([]);
  displayedColumns: string[] = [
    'name',
    'quantity',
    'color',
    'price',
    'actions',
  ];

  hatURL: string = 'https://steamcommunity.com/market/listings/440/';

  constructor(
    private readonly steamService: SteamService,
    private readonly cdr: ChangeDetectorRef,
    private readonly gameService: GameService,
    private readonly gameUrlService: GameUrlService,
  ) {
    // Reset URL + state when game changes
    effect(
      () => {
        void this.selectedGameId();
        this.gameUrlIdControl.setValue(null, { emitEvent: true });
        this.selectedMode.set(null);
        this.dataSource.data = [];
        this.statusLabel.set('');
        this.cdr.markForCheck();
      },
      { allowSignalWrites: true },
    );

    // Clear batch/state when URL changes
    effect(
      () => {
        void this.selectedGameUrl();
        this.clearBatchButtonClicked();
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
      throw new Error('gameUrlId is required');
    }
    return id;
  }

  private setSelectedMode(modeId: ScrapingMode): void {
    const mode = this.ScrapingModes.find((m) => m.id === modeId);
    if (!mode) {
      throw new Error('Invalid scraping mode');
    }
    this.selectedMode.set(mode);
  }

  private setLoading(): void {
    this.dataSource.data = [];
    this.isLoading.set(true);
    this.statusLabel.set('Loading...');
  }

  private finishLoading(): void {
    this.isLoading.set(false);
  }

  private executeScrape(
    mode: ScrapingMode,
    request$: () => any,
    successLabel: string,
    errorLabel: string,
  ): void {
    this.setSelectedMode(mode);

    if (this.selectedGameUrl() === null) {
      this.statusLabel.set('Game Url not selected');
      return;
    }

    this.setLoading();

    request$()
      .pipe(
        takeUntil(this.cancel$),
        takeUntilDestroyed(this.destroyRef),
        finalize(() => this.finishLoading()),
      )
      .subscribe({
        next: (response: Listing[]) => {
          this.dataSource.data = response ?? [];
          this.statusLabel.set(successLabel);
        },
        error: (err: any) => {
          this.statusLabel.set(err?.error ?? err?.message ?? errorLabel);
        },
      });
  }

  scrapeButtonClicked(): void {
    this.executeScrape(
      ScrapingMode.Scraper,
      () =>
        this.steamService.scrapePage(
          this.requireGameUrlId(),
          this.pageNumber(),
        ),
      'Successfully scraped Listings.',
      'Failed to scrape Listings.',
    );
  }

  publicApiScrapeButtonClicked(): void {
    this.executeScrape(
      ScrapingMode.PublicApiScraper,
      () =>
        this.steamService.scrapeFromPublicApi(
          this.requireGameUrlId(),
          this.pageNumber(),
        ),
      'Successfully scraped by Public API.',
      'Failed to scrape by Public API.',
    );
  }

  pixelScrapeButtonClicked(): void {
    this.executeScrape(
      ScrapingMode.PixelScrape,
      () =>
        this.steamService.scrapeForPixels(
          this.requireGameUrlId(),
          this.pageNumber(),
        ),
      'Successfully scraped by Pixel Scraper.',
      'Failed to scrape by Pixel Scraper.',
    );
  }

  retryBatchButtonClicked(): void {
    switch (this.selectedMode()?.id) {
      case ScrapingMode.Scraper:
        this.scrapeButtonClicked();
        break;
      case ScrapingMode.PublicApiScraper:
        this.publicApiScrapeButtonClicked();
        break;
      case ScrapingMode.PixelScrape:
        this.pixelScrapeButtonClicked();
        break;
    }
  }

  startBatchButtonClicked(): void {
    this.pageNumber.set(this.pageNumber() + 1);
    this.retryBatchButtonClicked();
  }

  onPageNumberChange(value: number): void {
    if (value < 1 || value > 100000) {
      this.pageNumber.set(1);
      return;
    }
    this.pageNumber.set(value);
  }

  cancelAll(): void {
    this.cancel$.next();
    this.statusLabel.set('Operation Cancelled.');
    this.isLoading.set(false);
  }

  clearButtonClicked(): void {
    this.dataSource.data = [];
    this.statusLabel.set('');
    this.gameIdControl.setValue(null);
    this.gameUrlIdControl.setValue(null);
    this.pageNumber.set(1);
  }

  clearBatchButtonClicked(): void {
    // batch reset logic (if needed)
  }

  exportButtonClicked(): void {
    const worksheet: XLSX.WorkSheet =
      XLSX.utils.json_to_sheet(this.dataSource.data);
    const workbook: XLSX.WorkBook = XLSX.utils.book_new();
    XLSX.utils.book_append_sheet(workbook, worksheet, 'Data');

    const today = new Date();
    XLSX.writeFile(
      workbook,
      `Export_${today.toDateString()}_Listings.xlsx`,
    );
  }
}