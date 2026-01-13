import { AfterViewInit, Component, OnDestroy, OnInit, ViewChild } from '@angular/core';
import { SteamService } from '../../services/steam/steam.service';
import { FormsModule } from '@angular/forms';
import { CommonModule } from '@angular/common';
import { MatTableDataSource } from '@angular/material/table';
import { MatTableModule } from '@angular/material/table';
import { MatPaginator, MatPaginatorModule } from '@angular/material/paginator';
import { MatSort, MatSortModule } from '@angular/material/sort';
import * as XLSX from 'xlsx';
import { Listing } from '../../models/listing.model';
import { finalize, Subject, takeUntil } from 'rxjs';
import { StopwatchComponent } from "../../components";


enum ScrapingMode {
  ClassicWebScraper = 1,
  ClassicWebScraperPaintsOnlyByPixel = 2,
  PublicApiDeserializer = 3,
  DeepClassicWebScraperPaintsOnly = 4
}

@Component({
    selector: 'web-scraper',
    imports: [
        FormsModule,
        CommonModule,
        MatTableModule,
        MatSort,
        MatSortModule,
        MatPaginatorModule,
        StopwatchComponent
    ],
    templateUrl: './web-scraper.component.html',
    styleUrl: './web-scraper.component.scss',
    providers: [SteamService]
})
export class WebScraperComponent implements OnInit, AfterViewInit, OnDestroy {
  @ViewChild(MatPaginator) paginator!: MatPaginator;
  @ViewChild(MatSort) sort!: MatSort;

  private cancel$ = new Subject<void>();

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
  lastUsedMode: number | null = null;

  constructor(private steamService: SteamService) {}

  ngOnInit(): void {}

  ngAfterViewInit(): void {
    if (!this.dataSource.paginator) {
      this.dataSource.paginator = this.paginator;
      this.dataSource.paginator.pageSize = 10;
      this.dataSource.paginator.pageSizeOptions = [10, 25, 50];
    }
    if (!this.dataSource.sort) {
      this.dataSource.sort = this.sort;
    }
  }

  retryBatchButtonClicked() {
    switch (this.lastUsedMode) {
      case ScrapingMode.ClassicWebScraper:
        this.classicWebScraperButtonClicked();
        break;
      case ScrapingMode.ClassicWebScraperPaintsOnlyByPixel:
        this.scrapedPageByPixelButtonClicked();
        break;
      case ScrapingMode.PublicApiDeserializer:
        this.publicApiDeserializerButtonClicked();
        break;

      case ScrapingMode.DeepClassicWebScraperPaintsOnly:
        this.deepClassicWebScraperPaintsOnlyButtonClicked();
        break;
    }
  }

  startBatchButtonClicked() {
    this.pageNumber++;

    switch (this.lastUsedMode) {
      case ScrapingMode.ClassicWebScraper:
        this.classicWebScraperButtonClicked();
        break;
      case ScrapingMode.ClassicWebScraperPaintsOnlyByPixel:
        this.scrapedPageByPixelButtonClicked();
        break;
      case ScrapingMode.PublicApiDeserializer:
        this.publicApiDeserializerButtonClicked();
        break;

      case ScrapingMode.DeepClassicWebScraperPaintsOnly:
        this.deepClassicWebScraperPaintsOnlyButtonClicked();
        break;
    }
  }

  classicWebScraperButtonClicked() {
    this.setLoading();
    this.steamService
      .getScrapedPage(this.pageNumber)
      .pipe(takeUntil(this.cancel$),
        finalize(() => {
          this.finishLoading();
        }))
      .subscribe({
        next: (response) => {
          if (response.length === 0) {
            console.log('No results found');
          }
          this.dataSource.data = response;
          console.log([response]);
          this.statusLabel = 'Successfully scraped Listings.';
        },
        error: (error) => {
          this.statusLabel = 'Failed to scrape Listings.';
          console.error('Error fetching data:', error);
        },
      });

    this.lastUsedMode = ScrapingMode.ClassicWebScraper;
  }

  scrapedPageByPixelButtonClicked() {
    this.setLoading();
    this.steamService
      .getScrapedPageByPixel(this.pageNumber)
      .pipe(takeUntil(this.cancel$),
        finalize(() => {
          this.finishLoading();
        }))
      .subscribe({
        next: (response) => {
          if (response.length === 0) {
            console.log('No results found');
          }

          this.dataSource.data = response;
          this.statusLabel =
            'Successfully scraped Listings with Paints by Pixel.';
          console.log([response]);
        },
        error: (error) => {
          this.statusLabel = 'Failed to scrape Listings with Paints by Pixel.';
          console.error('Error fetching data:', error);
        },
      });

    this.lastUsedMode = ScrapingMode.ClassicWebScraperPaintsOnlyByPixel;
  }

  publicApiDeserializerButtonClicked() {
    this.setLoading();
    this.steamService
      .getBulkPage(this.pageNumber)
      .pipe(takeUntil(this.cancel$),
        finalize(() => {
          this.finishLoading();
        }))
      .subscribe({
        next: (response) => {
          if (response.length === 0) {
            console.log('No results found');
          }
          this.dataSource.data = response;
          console.table(response);
          this.statusLabel = 'Successfully scraped by Public API.';
        },
        error: (error) => {
          this.statusLabel = 'Failed to scrape by Public API.';
          console.error('Error fetching data:', error);
        },
      });

    this.lastUsedMode = ScrapingMode.PublicApiDeserializer;
  }

  deepClassicWebScraperPaintsOnlyButtonClicked() {
    this.setLoading();
    this.steamService
      .getDeepScrapePaintedOnly(this.pageNumber)
      .pipe(takeUntil(this.cancel$),
        finalize(() => {
          this.finishLoading();
        }))
      .subscribe({
        next: (response) => {
          if (response.length === 0) {
            console.log('No results found');
          }

          this.dataSource.data = response;
          this.statusLabel = 'Successfully scraped by Deep Scraping.';
          console.log([response]);
        },
        error: (error) => {
          this.statusLabel = 'Failed to scrape by Deep Scraping.';
          console.error('Error fetching data:', error);
        },
      });

    this.lastUsedMode = ScrapingMode.DeepClassicWebScraperPaintsOnly;
  }

  checkPaintButtonClicked(name: string, index: number) {
    this.setLoadingWithoutDataClear();

    this.steamService
      .getIsHatPainted(name)
      .pipe(
        takeUntil(this.cancel$),
        finalize(() => {
          this.finishLoading();
        })
      )
      .subscribe({
        next: (response) => {
          console.log([response]);

          if (response.isPainted === false) {
            this.dataSource.data[index].color = 'Not Painted';
          } else {
            this.dataSource.data[index].color = response.paintText;
          }

          this.statusLabel = 'Successfully Check Listing for Paint.';
        },
        error: (error) => {
          this.statusLabel = 'Failed to Check Listing for Paint.';
          console.error('Error fetching data:', error);
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

  ngOnDestroy() {
    // Always complete on destroy to avoid leaks
    this.cancel$.next();
    this.cancel$.complete();
  }
}
