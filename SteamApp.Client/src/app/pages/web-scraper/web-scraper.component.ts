import {
  AfterViewInit,
  ChangeDetectorRef,
  Component,
  OnDestroy,
  OnInit,
  ViewChild,
} from '@angular/core';
import { SteamService } from '../../services/steam/steam.service';
import { FormControl, FormsModule } from '@angular/forms';
import { CommonModule } from '@angular/common';
import { MatTableDataSource } from '@angular/material/table';
import { MatTableModule } from '@angular/material/table';
import { MatPaginator, MatPaginatorModule } from '@angular/material/paginator';
import { MatSort, MatSortModule } from '@angular/material/sort';
import * as XLSX from 'xlsx';
import { Listing } from '../../models/listing.model';
import {
  BehaviorSubject,
  finalize,
  startWith,
  Subject,
  takeUntil,
  tap,
} from 'rxjs';
import { StopwatchComponent } from '../../components';
import { ComboBoxComponent } from '../../components/filter-components/combo-box-filter.component';
import { Game, GameUrl } from '../../models';
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
    FormsModule,
    CommonModule,
    MatTableModule,
    MatSort,
    MatSortModule,
    MatPaginatorModule,
    StopwatchComponent,
    ComboBoxComponent,
  ],
  templateUrl: './web-scraper.component.html',
  styleUrl: './web-scraper.component.scss',
  providers: [SteamService],
})
export class WebScraperComponent implements OnInit, AfterViewInit, OnDestroy {
  @ViewChild(MatPaginator) paginator!: MatPaginator;
  @ViewChild(MatSort) sort!: MatSort;

  private readonly destroy$ = new Subject<void>();
  private cancel$ = new Subject<void>();
  readonly gameIdControl = new FormControl<number | null>(null);
  readonly gameUrlIdControl = new FormControl<number | null>(null);

  readonly games$ = new BehaviorSubject<readonly Game[]>([]);
  readonly gameUrlsFiltered$ = new BehaviorSubject<readonly GameUrl[]>([]);
  selectedGameUrl: GameUrl | null = null;

  private games: Game[] = [];
  private gameUrlsAll: GameUrl[] = [];

  ScrapingModes: readonly ScrapingModeItem[] = [
    { id: ScrapingMode.Scraper, name: 'Web Scraper' },
    { id: ScrapingMode.PublicApiScraper, name: 'Public API Scrape' },
    { id: ScrapingMode.PixelScrape, name: 'Pixel Scrape' },
  ];

  selectedMode: ScrapingModeItem | null = null;

  dataSource = new MatTableDataSource<Listing>([]);
  displayedColumns: string[] = [
    'name',
    'quantity',
    'color',
    'price',
    'actions',
  ];

  hatURL: string = 'https://steamcommunity.com/market/listings/440/';

  pageNumber: number = 1;
  statusLabel: string = '';
  isLoading: boolean = false;

  constructor(
    private steamService: SteamService,
    private readonly cdr: ChangeDetectorRef,
    private readonly gameService: GameService,
    private readonly gameUrlService: GameUrlService,
  ) {}

  ngOnInit(): void {}

  ngAfterViewInit(): void {
    this.loadGames();
    this.loadGameUrls();

    if (!this.dataSource.paginator) {
      this.dataSource.paginator = this.paginator;
      this.dataSource.paginator.pageSize = 10;
      this.dataSource.paginator.pageSizeOptions = [10, 25, 50];
    }
    if (!this.dataSource.sort) {
      this.dataSource.sort = this.sort;
    }
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
        let filtered = urls.filter((x) => x.isBatchUrl);

        this.gameUrlsAll = filtered.map((url) => {
          if (url.isBatchUrl === true) {
            url.name += ' [ Batch ]';
          }
          if (url.isPixelScrape === true) {
            url.name += ' [ Pixel ]';
          }

          return url;
        });

        this.applyGameFilter(this.gameIdControl.value);
      });
  }

  retryBatchButtonClicked() {
    switch (this.selectedMode?.id) {
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

  startBatchButtonClicked() {
    this.pageNumber++;

    switch (this.selectedMode?.id) {
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

  scrapeButtonClicked() {
    this.executeScrape(
      ScrapingMode.Scraper,
      () =>
        this.steamService.scrapePage(this.requireGameUrlId(), this.pageNumber),
      'Successfully scraped Listings.',
      'Failed to scrape Listings.',
    );
  }

  publicApiScrapeButtonClicked() {
    this.executeScrape(
      ScrapingMode.PublicApiScraper,
      () =>
        this.steamService.scrapeFromPublicApi(
          this.requireGameUrlId(),
          this.pageNumber,
        ),
      'Successfully scraped by Public API.',
      'Failed to scrape by Public API.',
    );
  }

  pixelScrapeButtonClicked() {
    this.executeScrape(
      ScrapingMode.PixelScrape,
      () =>
        this.steamService.scrapeForPixels(
          this.requireGameUrlId(),
          this.pageNumber,
        ),
      'Successfully scraped by Pixel Scraper.',
      'Failed to scrape by Pixel Scraper.',
    );
  }

  private requireGameUrlId(): number {
    const id = this.gameUrlIdControl.value;

    if (id === null || id <= 0) {
      throw new Error('gameUrlId is required');
    }

    return id;
  }

  private executeScrape(
    mode: ScrapingMode,
    request: () => any,
    successLabel: string,
    errorLabel: string,
  ) {

    this.setSelectedMode(mode);

    if (this.selectedGameUrl === null) {
      this.statusLabel = 'Game Url not selected';
      return;
    }

    this.setLoading();

    request()
      .pipe(
        takeUntil(this.cancel$),
        finalize(() => this.finishLoading()),
      )
      .subscribe({
        next: (response: never[]) => {
          this.dataSource.data = response ?? [];
          this.statusLabel = successLabel;
        },
        error: (err:any) => {
          this.statusLabel = err?.error ?? err?.message ?? 'Unknown error';
        },
      });
  }

  exportButtonClicked(): void {
    const dataToExport = this.dataSource.data;

    const worksheet: XLSX.WorkSheet = XLSX.utils.json_to_sheet(dataToExport);

    const workbook: XLSX.WorkBook = XLSX.utils.book_new();
    XLSX.utils.book_append_sheet(workbook, worksheet, 'Data');

    const today = new Date();
    XLSX.writeFile(workbook, `Export_${today.toDateString()}_Listings.xlsx`);
  }

  clearButtonClicked(): void {
    this.dataSource.data = [];
    this.statusLabel = '';

    this.gameIdControl.setValue(null);
    this.gameUrlIdControl.setValue(null);

    this.onPageNumberChange(1);
  }

  setLoading(): void {
    this.dataSource.data = [];
    this.isLoading = true;
    this.statusLabel = 'Loading...';
  }

  setLoadingWithoutDataClear(): void {
    this.isLoading = true;
    this.statusLabel = 'Loading...';
  }

  finishLoading(): void {
    this.isLoading = false;
  }

  onPageNumberChange(value: number) {
    if (value < 0 || value > 100000) {
      this.pageNumber = 1;
    } else {
      this.pageNumber = value;
    }
  }

  cancelAll() {
    this.cancel$.next();

    this.statusLabel = 'Operation Cancelled.';
    this.finishLoading();

    this.cancel$ = new Subject<void>();
  }

  readonly bindGameUrlToExternal$ = this.gameUrlIdControl.valueChanges.pipe(
    startWith(this.gameUrlIdControl.value),
    tap((gameUrlId) => {
      if (gameUrlId === null) {
        this.selectedGameUrl = null;
        this.cdr.markForCheck();
        return;
      }

      this.selectedGameUrl =
        this.gameUrlsFiltered$.value.find((u) => u.id === gameUrlId) ?? null;

      this.clearBatchButtonClicked();

      // if (this.selectedGameUrl?.isBatchUrl) {
      //   this.currentIndex = this.selectedGameUrl.startPage ?? null;
      //   this.batchSize = this.currentIndex === null ? null : 5;
      // }

      this.cdr.markForCheck();
    }),
  );

  clearBatchButtonClicked(): void {
    // this.currentIndex = null;
    // this.batchSize = null;
  }

  readonly bindToExternal$ = this.gameIdControl.valueChanges.pipe(
    startWith(this.gameIdControl.value),
    tap((gameId) => {
      this.applyGameFilter(gameId);
    }),
  );

  private applyGameFilter(gameId: number | null): void {
    if (gameId === null) {
      this.gameUrlsFiltered$.next([]);
      this.gameUrlIdControl.reset();
      this.selectedGameUrl = null;

      this.cdr.markForCheck();
      return;
    }

    const urls = this.gameUrlsAll.filter((url) => url.gameId === gameId);
    this.gameUrlsFiltered$.next(urls);

    this.gameUrlIdControl.reset();
    this.selectedGameUrl = null;

    this.cdr.markForCheck();
  }

  private setSelectedMode(modeId: ScrapingMode): void {
    const mode = this.ScrapingModes.find((m) => m.id === modeId);

    if (!mode) {
      throw new Error('Invalid scraping mode');
    }

    this.selectedMode = mode;
  }

  ngOnDestroy() {
    // Always complete on destroy to avoid leaks
    this.cancel$.next();
    this.cancel$.complete();
  }
}
