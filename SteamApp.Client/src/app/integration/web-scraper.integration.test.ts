import { ComponentFixture, TestBed } from '@angular/core/testing';
import { NoopAnimationsModule } from '@angular/platform-browser/animations';
import { of } from 'rxjs';

import { Game, GameUrl, Listing, ScrapingMode, ScrapingModeEnum } from '../models';
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
  };

  beforeEach(async () => {
    steamService = {
      scrapePage: jest.fn(() => of(listings)),
      scrapeFromPublicApi: jest.fn(() => of([])),
      scrapeForPixels: jest.fn(() => of([])),
    };

    await TestBed.configureTestingModule({
      imports: [WebScraperComponent, NoopAnimationsModule],
      providers: [
        { provide: GameService, useValue: { getAll: () => of(games) } },
        { provide: GameUrlService, useValue: { getAll: () => of(gameUrls) } },
        { provide: ScrapingModeService, useValue: { getAll: () => of(scrapingModes) } },
      ],
    })
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

    component.runButtonClicked();
    fixture.detectChanges();

    expect(steamService.scrapePage).toHaveBeenCalledWith(10, 1);
    expect(component.dataSource.data).toEqual(listings);
    expect(component.statusLabel()).toBe('Successfully ran Web Scrape on page 1.');
    expect((fixture.nativeElement.textContent as string)).toContain('Alpha Item');
  });
});
