import { ComponentFixture, TestBed } from '@angular/core/testing';
import { MAT_DIALOG_DATA, MatDialogRef } from '@angular/material/dialog';
import { of } from 'rxjs';

import { ManualCheckRunDetail, ManualCheckRunSummary } from '../../models';
import { ManualCheckService } from '../../services';
import { ManualCheckHistoryDialogComponent } from './manual-check-history-dialog.component';

describe('ManualCheckHistoryDialogComponent', () => {
  let fixture: ComponentFixture<ManualCheckHistoryDialogComponent>;
  let component: ManualCheckHistoryDialogComponent;
  let service: jasmine.SpyObj<ManualCheckService>;

  const failedRun: ManualCheckRunSummary = {
    id: 42,
    presetId: 3,
    presetName: 'Mean Green',
    gameId: 440,
    gameName: 'Team Fortress 2',
    gameUrlId: 8,
    gameUrlName: 'Steam Market',
    totalProducts: 2,
    checkedProducts: 2,
    matchedProducts: 0,
    failedProducts: 2,
    progress: {
      totalProducts: 2,
      checkedProducts: 2,
      matchedProducts: 0,
      failedProducts: 2,
    },
    status: 'Failed',
    date: '2026-08-15T10:00:00Z',
    startedAtUtc: '2026-08-15T10:00:01Z',
    completedAtUtc: '2026-08-15T10:00:10Z',
    correlationId: 'trace-42',
    errorText: 'All 2 product checks failed. Most common error: Steam returned HTTP 429.',
  };

  const detail: ManualCheckRunDetail = {
    ...failedRun,
    setup: {
      presetId: 3,
      presetName: 'Mean Green',
      gameId: 440,
      gameName: 'Team Fortress 2',
      gameUrlId: 8,
      gameUrlName: 'Steam Market',
      matchMode: 'Any',
      criteria: [{ nameContains: null, valueContains: 'Mean Green' }],
      products: [],
      requestedAtUtc: '2026-08-15T10:00:00Z',
    },
    results: {
      matches: [],
      errors: [{
        productId: 1,
        productName: 'Rocket Launcher',
        fullUrl: 'https://steamcommunity.com/market/listings/440/Rocket%20Launcher',
        error: 'Steam returned HTTP 429 (Too Many Requests).',
        errorType: 'HttpRequestException',
        httpStatusCode: 429,
        occurredAtUtc: '2026-08-15T10:00:09Z',
      }],
    },
  };

  beforeEach(async () => {
    service = jasmine.createSpyObj<ManualCheckService>('ManualCheckService', [
      'getRuns',
      'getRun',
      'rerun',
    ]);
    service.getRuns.and.returnValue(of([failedRun]));
    service.getRun.and.returnValue(of(detail));

    await TestBed.configureTestingModule({
      imports: [ManualCheckHistoryDialogComponent],
      providers: [
        { provide: ManualCheckService, useValue: service },
        { provide: MatDialogRef, useValue: jasmine.createSpyObj('MatDialogRef', ['close']) },
        { provide: MAT_DIALOG_DATA, useValue: {} },
      ],
    }).compileComponents();

    fixture = TestBed.createComponent(ManualCheckHistoryDialogComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('shows the run failure and correlation trace in the history table', () => {
    const text = fixture.nativeElement.textContent;

    expect(text).toContain('All 2 product checks failed');
    expect(text).toContain('Trace trace-42');
  });

  it('renders a readable product-level error trace instead of raw JSON', () => {
    component.openViewer(failedRun, 'errors');
    fixture.detectChanges();

    const text = fixture.nativeElement.textContent;
    expect(service.getRun).toHaveBeenCalledWith(42);
    expect(text).toContain('Rocket Launcher');
    expect(text).toContain('HTTP 429');
    expect(text).toContain('HttpRequestException');
    expect(text).toContain('Steam returned HTTP 429');
  });
});
