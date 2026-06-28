import { ComponentFixture, fakeAsync, TestBed, tick } from '@angular/core/testing';
import { MatDialog, MatDialogRef } from '@angular/material/dialog';
import { NoopAnimationsModule } from '@angular/platform-browser/animations';
import { of } from 'rxjs';

import { ScrapeHistory } from '../models';
import { ScrapeHistoryDialogComponent } from '../pages/web-scraper/scrape-history-dialog.component';
import { SteamService } from '../services';

describe('ScrapeHistoryDialogComponent integration tests', () => {
  const history: ScrapeHistory[] = [
    {
      id: 7,
      endpoint: 'scrape-pixels',
      scrapeType: 'Pixel Scrape',
      gameUrlId: 10,
      gameUrlName: 'Pixel URL',
      page: 2,
      resultCount: 0,
      date: new Date('2026-05-16T10:00:00Z').toISOString(),
      isHaveError: true,
    },
  ];

  let fixture: ComponentFixture<ScrapeHistoryDialogComponent>;
  let component: ScrapeHistoryDialogComponent;
  let steamService: {
    getScrapeHistory: jest.Mock;
    getScrapeHistoryDetail: jest.Mock;
    rerunScrapeHistory: jest.Mock;
  };
  let dialog: {
    open: jest.Mock;
  };
  let dialogRef: {
    close: jest.Mock;
  };

  beforeEach(async () => {
    steamService = {
      getScrapeHistory: jest.fn(() => of(history)),
      getScrapeHistoryDetail: jest.fn(() =>
        of({
          ...history[0],
          setupJson: '{"gameUrlId":10,"page":2}',
          resultsJson: '[{"name":"Alpha"}]',
          errorText: 'Steam failed',
        }),
      ),
      rerunScrapeHistory: jest.fn(() =>
        of({
          history: { ...history[0], isHaveError: false, resultCount: 1 },
          results: [{ name: 'Alpha', price: 1, imageUrl: '', quantity: 1, pixelName: '', linkUrl: '', pageUrl: '', redValue: null, greenValue: null, blueValue: null, isPainted: false }],
        }),
      ),
    };
    dialog = {
      open: jest.fn(),
    };
    dialogRef = {
      close: jest.fn(),
    };

    await TestBed.configureTestingModule({
      imports: [ScrapeHistoryDialogComponent, NoopAnimationsModule],
      providers: [
        { provide: SteamService, useValue: steamService },
      ],
    })
      .overrideProvider(MatDialog, { useValue: dialog })
      .overrideProvider(MatDialogRef, { useValue: dialogRef })
      .compileComponents();

    fixture = TestBed.createComponent(ScrapeHistoryDialogComponent);
    component = fixture.componentInstance;
  });

  function initializeDialog(): void {
    fixture.detectChanges();
    tick();
  }

  it('renders historical rows with action buttons', fakeAsync(() => {
    initializeDialog();

    const text = fixture.nativeElement.textContent as string;
    const buttons = fixture.nativeElement.querySelectorAll('button');

    expect(text).toContain('Pixel Scrape');
    expect(text).toContain('Pixel URL');
    expect(text).toContain('Error');
    expect(buttons.length).toBeGreaterThanOrEqual(5);
  }));

  it('opens setup, results, and error detail dialogs', fakeAsync(() => {
    initializeDialog();

    component.openSetup(history[0]);
    component.openResults(history[0]);
    component.openError(history[0]);

    expect(steamService.getScrapeHistoryDetail).toHaveBeenCalledTimes(3);
    expect(dialog.open).toHaveBeenCalledTimes(3);
  }));

  it('closes with rerun response', fakeAsync(() => {
    initializeDialog();

    component.rerun(history[0]);

    expect(steamService.rerunScrapeHistory).toHaveBeenCalledWith(7);
    expect(dialogRef.close).toHaveBeenCalledWith(expect.objectContaining({
      results: expect.any(Array),
    }));
  }));
});
