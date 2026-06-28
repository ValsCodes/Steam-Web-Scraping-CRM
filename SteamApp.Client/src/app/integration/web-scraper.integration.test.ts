import { ComponentFixture, TestBed } from '@angular/core/testing';
import { MatDialog } from '@angular/material/dialog';
import { NoopAnimationsModule } from '@angular/platform-browser/animations';
import { of } from 'rxjs';

import {
  Game,
  GameUrl,
  Listing,
  ScrapeHistoryRerunResponse,
  ScrapingMode,
  ScrapingModeEnum,
} from '../models';
import { WebScraperComponent } from '../pages/web-scraper/web-scraper.component';
import { GameUrlService, ScrapingModeService } from '../services';
import { GameService } from '../services/game/game.service';
import { SteamService } from '../services/steam/steam.service';

describe('WebScraperComponent integration tests', () => {
  const games: Game[] = [
    {
      id: 1,
      name: 'Alpha Game',
      baseUrl: 'https://steamcommunity.com',
      pageUrl: 'https://steamcommunity.com/app/440',
      internalId: 440,
      isActive: true,
    },
    {
      id: 2,
      name: 'Inactive Game',
      baseUrl: 'https://steamcommunity.com',
      pageUrl: 'https://steamcommunity.com/app/730',
      internalId: 730,
      isActive: false,
    },
  ];
  const gameUrls: GameUrl[] = [
    {
      id: 10,
      name: 'Batch URL',
      gameId: 1,
      gameName: 'Alpha Game',
      scrapingModeId: ScrapingModeEnum.Batch,
      scrapingModeName: 'Batch',
      partialUrl: 'https://steamcommunity.com/market/search?page={0}',
      isActive: true,
    },
  ];
  const scrapingModes: ScrapingMode[] = [
    { id: ScrapingModeEnum.ManualBatch, name: 'Manual Batch' },
    { id: ScrapingModeEnum.Batch, name: 'Batch' },
    { id: ScrapingModeEnum.PixelBatch, name: 'Pixel Batch' },
  ];
  const listings: Listing[] = [
    {
      name: 'Alpha Item',
      price: 1.23,
      imageUrl: 'https://steamcommunity.com/image.png',
      quantity: 3,
      pixelName: '',
      linkUrl: 'https://steamcommunity.com/market/listings/440/Alpha%20Item',
      pageUrl: 'https://steamcommunity.com/market/search?page=1',
      redValue: null,
      greenValue: null,
      blueValue: null,
      isPainted: false,
    },
  ];

  let fixture: ComponentFixture<WebScraperComponent>;
  let component: WebScraperComponent;
  let steamService: {
    scrapePage: jest.Mock;
    scrapeFromPublicApi: jest.Mock;
    scrapeForPixels: jest.Mock;
    getScrapeHistory: jest.Mock;
    getScrapeHistoryDetail: jest.Mock;
    rerunScrapeHistory: jest.Mock;
  };
  let dialog: {
    open: jest.Mock;
  };

  beforeEach(async () => {
    steamService = {
      scrapePage: jest.fn(() => of(listings)),
      scrapeFromPublicApi: jest.fn(() => of([])),
      scrapeForPixels: jest.fn(() => of([])),
      getScrapeHistory: jest.fn(() => of([])),
      getScrapeHistoryDetail: jest.fn(() => of({})),
      rerunScrapeHistory: jest.fn(() => of({ history: {}, results: [] })),
    };
    dialog = {
      open: jest.fn(() => ({ afterClosed: () => of(undefined) })),
    };

    await TestBed.configureTestingModule({
      imports: [WebScraperComponent, NoopAnimationsModule],
      providers: [
        { provide: GameService, useValue: { getAll: () => of(games) } },
        { provide: GameUrlService, useValue: { getAll: () => of(gameUrls) } },
        { provide: ScrapingModeService, useValue: { getAll: () => of(scrapingModes) } },
      ],
    })
      .overrideProvider(MatDialog, { useValue: dialog })
      .overrideComponent(WebScraperComponent, {
        set: {
          providers: [{ provide: SteamService, useValue: steamService }],
        },
      })
      .compileComponents();

    fixture = TestBed.createComponent(WebScraperComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('loads only active games and supported scraper modes', () => {
    expect(component.games().map((game) => game.name)).toEqual(['Alpha Game']);
    expect(component.scrapingModes().map((mode) => mode.id)).toEqual([
      ScrapingModeEnum.Batch,
      ScrapingModeEnum.PixelBatch,
    ]);
  });

  it('filters URLs by selected game and scraping mode', () => {
    component.gameIdControl.setValue(1);
    fixture.detectChanges();
    component.scrapingModeIdControl.setValue(ScrapingModeEnum.Batch);
    fixture.detectChanges();

    expect(component.gameUrlsFiltered().map((url) => url.name)).toEqual(['Batch URL']);
  });

  it('prefixes the Show Page URL when Steam mode is checked', () => {
    component.gameIdControl.setValue(1);
    fixture.detectChanges();
    component.scrapingModeIdControl.setValue(ScrapingModeEnum.Batch);
    fixture.detectChanges();
    component.gameUrlIdControl.setValue(10);
    fixture.detectChanges();
    component.pageNumber.set(3);
    component.openInSteamMode.set(true);
    fixture.detectChanges();

    expect(component.getSafeShowPageUrl()).toBe(
      'steam://openurl/https://steamcommunity.com/market/search?page=3',
    );
  });

  it('runs the selected web scrape and renders the returned listings', () => {
    component.gameIdControl.setValue(1);
    fixture.detectChanges();
    component.scrapingModeIdControl.setValue(ScrapingModeEnum.Batch);
    fixture.detectChanges();
    component.gameUrlIdControl.setValue(10);
    fixture.detectChanges();
    component.setSelectedMode(
      ScrapingModeEnum.Batch as unknown as Parameters<WebScraperComponent['setSelectedMode']>[0],
    );
    component.openInSteamMode.set(true);

    component.runButtonClicked();
    fixture.detectChanges();

    expect(steamService.scrapePage).toHaveBeenCalledWith(10, 1);
    expect(component.dataSource.data).toEqual(listings);
    expect(component.statusLabel()).toBe('Successfully ran Web Scrape on page 1.');
    expect((fixture.nativeElement.textContent as string)).toContain('Alpha Item');
  });

  it('opens scrape history from the toolbar', () => {
    component.historyButtonClicked();

    expect(dialog.open).toHaveBeenCalled();
  });

  it('applies rerun results returned from history dialog', () => {
    const rerunResponse: ScrapeHistoryRerunResponse = {
      history: {
        id: 1,
        endpoint: 'scrape-page',
        scrapeType: 'Web Scrape',
        gameUrlId: 10,
        gameUrlName: 'Batch URL',
        page: 3,
        resultCount: 1,
        date: new Date().toISOString(),
        isHaveError: false,
      },
      results: [{ ...listings[0], name: 'Rerun Item' }],
    };
    dialog.open.mockReturnValue({ afterClosed: () => of(rerunResponse) });

    component.historyButtonClicked();
    fixture.detectChanges();

    expect(component.dataSource.data.map((item) => item.name)).toEqual(['Rerun Item']);
    expect(component.statusLabel()).toBe('Reran Web Scrape from history on page 3.');
  });
});
