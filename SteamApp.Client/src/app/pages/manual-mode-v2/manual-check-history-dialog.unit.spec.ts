import { ComponentFixture, TestBed } from '@angular/core/testing';
import { MAT_DIALOG_DATA, MatDialog, MatDialogRef } from '@angular/material/dialog';
import { of } from 'rxjs';

import { ManualCheckRunSummary } from '../../models';
import { ManualCheckService } from '../../services';
import { ManualCheckHistoryDialogComponent } from './manual-check-history-dialog.component';
import {
  ManualCheckTraceDialogComponent,
  ManualCheckTraceDialogData,
} from './manual-check-trace-dialog.component';

describe('ManualCheckHistoryDialogComponent', () => {
  let fixture: ComponentFixture<ManualCheckHistoryDialogComponent>;
  let component: ManualCheckHistoryDialogComponent;
  let service: jasmine.SpyObj<ManualCheckService>;
  let openDialog: jasmine.Spy;

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
    durationMilliseconds: 9000,
    correlationId: 'trace-42',
    errorText: 'All 2 product checks failed. Most common error: Steam returned HTTP 429.',
  };

  beforeEach(async () => {
    service = jasmine.createSpyObj<ManualCheckService>('ManualCheckService', ['getRuns', 'rerun']);
    service.getRuns.and.returnValue(of([failedRun]));

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
    const dialogOwner = component as unknown as { dialog: MatDialog };
    openDialog = spyOn(dialogOwner.dialog, 'open');
    fixture.detectChanges();
  });

  it('shows failure, correlation, and duration in the history table without inline details', () => {
    const text = fixture.nativeElement.textContent;

    expect(text).toContain('All 2 product checks failed');
    expect(text).toContain('Trace trace-42');
    expect(text).toContain('9.0 s');
    expect(fixture.nativeElement.querySelector('.manual-check-history__viewer')).toBeNull();
  });

  it('opens failed history traces in the separate result dialog', () => {
    component.openTrace(failedRun);

    expect(openDialog).toHaveBeenCalledWith(
      ManualCheckTraceDialogComponent,
      jasmine.objectContaining<Partial<{ data: ManualCheckTraceDialogData }>>({
        data: { runId: 42, initialView: 'errors' },
      }),
    );
  });

  it('opens successful history traces on the checks view', () => {
    const succeeded: ManualCheckRunSummary = {
      ...failedRun,
      status: 'Succeeded',
      failedProducts: 0,
      errorText: null,
    };

    component.openTrace(succeeded);

    expect(openDialog).toHaveBeenCalledWith(
      ManualCheckTraceDialogComponent,
      jasmine.objectContaining<Partial<{ data: ManualCheckTraceDialogData }>>({
        data: { runId: 42, initialView: 'results' },
      }),
    );
  });
});
