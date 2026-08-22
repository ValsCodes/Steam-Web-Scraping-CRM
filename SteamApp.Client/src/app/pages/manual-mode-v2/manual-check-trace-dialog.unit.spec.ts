import { HttpErrorResponse, HttpHeaders } from '@angular/common/http';
import { ComponentFixture, TestBed } from '@angular/core/testing';
import { MAT_DIALOG_DATA, MatDialog, MatDialogRef } from '@angular/material/dialog';
import { of, throwError } from 'rxjs';

import { ManualCheckRunDetail } from '../../models';
import { ManualCheckService } from '../../services';
import { ManualCheckSteamResultDialogComponent } from './manual-check-steam-result-dialog.component';
import {
  ManualCheckTraceDialogComponent,
  ManualCheckTraceDialogData,
} from './manual-check-trace-dialog.component';

describe('ManualCheckTraceDialogComponent', () => {
  let fixture: ComponentFixture<ManualCheckTraceDialogComponent>;
  let component: ManualCheckTraceDialogComponent;
  let service: jasmine.SpyObj<ManualCheckService>;
  let dialogData: ManualCheckTraceDialogData;
  let openDialog: jasmine.Spy;

  const productTrace = {
    productId: 1,
    productName: 'Rocket Launcher',
    fullUrl: 'https://steamcommunity.com/market/listings/440/Rocket%20Launcher',
    matchEvaluated: false,
    matched: false,
    matchedAssetCount: 0,
    steamApiResultJson: '{"success":true,"total_count":1}',
    durationMilliseconds: 1250,
  };

  const detail: ManualCheckRunDetail = {
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
    failedProducts: 1,
    progress: {
      totalProducts: 2,
      checkedProducts: 2,
      matchedProducts: 0,
      failedProducts: 1,
    },
    status: 'CompletedWithErrors',
    date: '2026-08-15T10:00:00Z',
    startedAtUtc: '2026-08-15T10:00:01Z',
    completedAtUtc: '2026-08-15T10:00:10Z',
    durationMilliseconds: 9000,
    correlationId: 'trace-42',
    errorText: 'One product check failed.',
    setup: {
      presetId: 3,
      presetName: 'Mean Green',
      gameId: 440,
      gameName: 'Team Fortress 2',
      gameUrlId: 8,
      gameUrlName: 'Steam Market',
      listingLimit: 37,
      cooldownMinutes: 1,
      cooldownSeconds: 9,
      bypassCache: false,
      requestedProductIds: null,
      criteria: [
        { conditionOperatorId: null, nameContains: null, valueContains: 'Mean Green' },
        { conditionOperatorId: 3, conditionOperatorName: 'AND NOT', nameContains: 'Wear', valueContains: 'Battle Scarred' },
      ],
      products: [],
      requestedAtUtc: '2026-08-15T10:00:00Z',
    },
    results: {
      matches: [],
      productTraces: [productTrace],
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
    dialogData = { runId: 42, initialView: 'errors' };
    service = jasmine.createSpyObj<ManualCheckService>('ManualCheckService', ['getRun']);
    service.getRun.and.returnValue(of(detail));

    await TestBed.configureTestingModule({
      imports: [ManualCheckTraceDialogComponent],
      providers: [
        { provide: ManualCheckService, useValue: service },
        { provide: MatDialogRef, useValue: jasmine.createSpyObj('MatDialogRef', ['close']) },
        { provide: MAT_DIALOG_DATA, useFactory: () => dialogData },
      ],
    }).compileComponents();
  });

  function createComponent(): void {
    fixture = TestBed.createComponent(ManualCheckTraceDialogComponent);
    component = fixture.componentInstance;
    const dialogOwner = component as unknown as { dialog: MatDialog };
    openDialog = spyOn(dialogOwner.dialog, 'open');
    fixture.detectChanges();
  }

  it('loads a history run by ID and opens on the requested error view', () => {
    createComponent();

    const text = fixture.nativeElement.textContent;
    expect(service.getRun).toHaveBeenCalledOnceWith(42);
    expect(component.view).toBe('errors');
    expect(text).toContain('Automated Check Result · Run #42');
    expect(text).toContain('Rocket Launcher');
    expect(text).toContain('HTTP 429');
    expect(text).toContain('9.0 s');
  });

  it('renders an already-loaded current result without another request', () => {
    dialogData = { detail, initialView: 'setup' };
    createComponent();

    const text = fixture.nativeElement.textContent;
    expect(service.getRun).not.toHaveBeenCalled();
    expect(text).toContain('Top 37 cheapest available listing(s) checked per product.');
    expect(text).toContain('1m 9s');
    expect(text).toContain('AND NOT');
    expect(text).toContain('Steam data: 20-minute cache allowed.');
  });

  it('renders the saved nested setup expression', () => {
    const groupedDetail: ManualCheckRunDetail = {
      ...detail,
      setup: {
        ...detail.setup,
        criteria: [
          { conditionOperatorId: null, nameContains: 'A', valueContains: 'A', openGroupCount: 0, closeGroupCount: 0 },
          { conditionOperatorId: 1, conditionOperatorName: 'AND', nameContains: 'B', valueContains: 'B', openGroupCount: 1, closeGroupCount: 0 },
          { conditionOperatorId: 2, conditionOperatorName: 'OR', nameContains: 'C', valueContains: 'C', openGroupCount: 0, closeGroupCount: 1 },
        ],
      },
    };
    dialogData = { detail: groupedDetail, initialView: 'setup' };
    createComponent();

    expect(fixture.nativeElement.textContent).toContain('[A: A] AND ([B: B] OR [C: C])');
    expect(component.setupExpressionValid).toBeTrue();
  });

  it('falls back to a flat setup list when historical grouping is malformed', () => {
    const malformedDetail: ManualCheckRunDetail = {
      ...detail,
      setup: {
        ...detail.setup,
        criteria: [
          { conditionOperatorId: null, nameContains: 'A', valueContains: 'A', closeGroupCount: 1 },
          { conditionOperatorId: 1, conditionOperatorName: 'AND', nameContains: 'B', valueContains: 'B' },
        ],
      },
    };
    dialogData = { detail: malformedDetail, initialView: 'setup' };
    createComponent();

    expect(component.setupExpressionValid).toBeFalse();
    expect(fixture.nativeElement.textContent).toContain('Criteria are shown as a flat list');
    expect(fixture.nativeElement.textContent).toContain('[A: A] AND [B: B]');
  });

  it('shows product checks, durations, and Steam-result actions', () => {
    dialogData = { detail, initialView: 'results' };
    createComponent();

    const actions = fixture.nativeElement.querySelector(
      '[aria-label="Open Steam result actions for Rocket Launcher"]',
    );
    expect(fixture.nativeElement.textContent).toContain('Check failed');
    expect(fixture.nativeElement.textContent).toContain('1.3 s');
    expect(actions).not.toBeNull();
  });

  it('opens the Steam API result dialog for the selected trace', () => {
    dialogData = { detail, initialView: 'results' };
    createComponent();

    component.openSteamResult(productTrace);

    expect(openDialog).toHaveBeenCalledWith(
      ManualCheckSteamResultDialogComponent,
      jasmine.objectContaining({ data: productTrace }),
    );
  });

  it('shows a retryable request error', () => {
    service.getRun.and.returnValue(throwError(() => new HttpErrorResponse({
      status: 429,
      headers: new HttpHeaders({ 'Retry-After': '12' }),
    })));
    createComponent();

    expect(fixture.nativeElement.textContent).toContain('Too many requests. Try again in 12 seconds.');
    expect(fixture.nativeElement.textContent).toContain('Try again');
  });
});
