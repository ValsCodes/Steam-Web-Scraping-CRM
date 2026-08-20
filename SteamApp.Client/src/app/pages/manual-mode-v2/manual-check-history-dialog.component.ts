import { CommonModule } from '@angular/common';
import { HttpErrorResponse } from '@angular/common/http';
import {
  ChangeDetectionStrategy,
  ChangeDetectorRef,
  Component,
  inject,
  OnInit,
} from '@angular/core';
import { MatButtonModule } from '@angular/material/button';
import {
  MAT_DIALOG_DATA,
  MatDialog,
  MatDialogModule,
  MatDialogRef,
} from '@angular/material/dialog';
import { finalize } from 'rxjs';

import { formatDuration } from '../../common';
import { ManualCheckRunSummary } from '../../models';
import { ManualCheckService } from '../../services';
import {
  ManualCheckTraceDialogComponent,
  ManualCheckTraceDialogData,
  ManualCheckTraceView,
} from './manual-check-trace-dialog.component';

export interface ManualCheckHistoryDialogData {
  gameId?: number;
}

@Component({
  selector: 'steam-manual-check-history-dialog',
  standalone: true,
  changeDetection: ChangeDetectionStrategy.OnPush,
  imports: [CommonModule, MatDialogModule, MatButtonModule],
  template: `
    <h2 mat-dialog-title>Automated Check History</h2>
    <mat-dialog-content class="manual-check-history">
      <p class="manual-check-history__intro">
        Every run keeps its setup, timestamps, durations, correlation ID, matches, parsed Steam results, and product-level failure trace.
      </p>

      @if (loading) {
        <div class="manual-check-history__loading" role="status">Loading history…</div>
      } @else if (errorMessage) {
        <div class="manual-check-history__callout manual-check-history__callout--error" role="alert">
          <strong>History could not be loaded</strong>
          <span>{{ errorMessage }}</span>
          <button mat-stroked-button type="button" (click)="load()">Try again</button>
        </div>
      } @else if (runs.length === 0) {
        <div class="manual-check-history__empty">
          <strong>No automated checks yet</strong>
          <span>Completed and failed checks will appear here with their trace.</span>
        </div>
      } @else {
        <div class="manual-check-history__table-wrap">
          <table>
            <thead>
              <tr>
                <th>Run</th>
                <th>Game / source</th>
                <th>Preset</th>
                <th>Status</th>
                <th>Progress</th>
                <th>Duration</th>
                <th>Outcome</th>
                <th>Failure trace</th>
                <th>Actions</th>
              </tr>
            </thead>
            <tbody>
              @for (run of runs; track run.id) {
                <tr
                  [class.manual-check-history__row--failed]="run.status === 'Failed'"
                  [class.manual-check-history__row--canceled]="run.status === 'Canceled'">
                  <td>
                    <strong>#{{ run.id }}</strong><br />
                    <small>{{ run.date | date:'medium' }}</small>
                  </td>
                  <td>
                    {{ run.gameName || ('Game #' + run.gameId) }}<br />
                    <small>{{ run.gameUrlName || ('Source #' + run.gameUrlId) }}</small>
                  </td>
                  <td>{{ run.presetName }}</td>
                  <td>
                    <span [class]="'manual-check-history__status manual-check-history__status--' + run.status.toLowerCase()">
                      {{ statusLabel(run) }}
                    </span>
                  </td>
                  <td>{{ run.checkedProducts }} / {{ run.totalProducts }}</td>
                  <td>{{ formatDuration(run.durationMilliseconds) }}</td>
                  <td>
                    <span class="manual-check-history__outcome--match">{{ run.matchedProducts }} matched</span><br />
                    <span [class.manual-check-history__outcome--failed]="run.failedProducts > 0">
                      {{ run.failedProducts }} failed
                    </span>
                  </td>
                  <td class="manual-check-history__trace-summary">
                    @if (run.errorText) {
                      <span>{{ run.errorText }}</span>
                    } @else if (run.failedProducts > 0) {
                      <span>{{ run.failedProducts }} product error(s) recorded.</span>
                    } @else {
                      <span class="manual-check-history__muted">No errors</span>
                    }
                    <small title="Correlation ID">Trace {{ run.correlationId }}</small>
                  </td>
                  <td class="manual-check-history__actions">
                    <button mat-stroked-button type="button" (click)="openTrace(run)">View trace</button>
                    <button
                      mat-button
                      type="button"
                      (click)="rerun(run)"
                      [disabled]="busyRunId !== null">
                      {{ busyRunId === run.id ? 'Starting…' : 'Rerun' }}
                    </button>
                  </td>
                </tr>
              }
            </tbody>
          </table>
        </div>
      }
    </mat-dialog-content>
    <mat-dialog-actions align="end">
      <button mat-button type="button" (click)="load()" [disabled]="loading">Refresh</button>
      <button mat-flat-button type="button" (click)="dialogRef.close()">Close</button>
    </mat-dialog-actions>
  `,
  styles: [`
    .manual-check-history { min-width: min(74rem, 90vw); }
    .manual-check-history__intro { margin: 0 0 1rem; color: #64748b; }
    .manual-check-history__loading, .manual-check-history__empty { display: flex; min-height: 12rem; flex-direction: column; align-items: center; justify-content: center; gap: .4rem; color: #64748b; text-align: center; }
    .manual-check-history__table-wrap { overflow: auto; max-height: 65vh; border: 1px solid #e2e8f0; border-radius: .5rem; }
    table { width: 100%; border-collapse: collapse; font-size: .875rem; }
    th, td { border-bottom: 1px solid #e2e8f0; padding: .7rem; text-align: left; vertical-align: top; }
    th { color: #475569; position: sticky; top: 0; z-index: 1; background: #f8fafc; }
    tbody tr:last-child td { border-bottom: 0; }
    small, .manual-check-history__muted { color: #64748b; }
    .manual-check-history__row--failed { background: #fffafa; }
    .manual-check-history__row--canceled { background: #fffbeb; }
    .manual-check-history__actions { display: flex; flex-direction: column; align-items: stretch; gap: .25rem; white-space: nowrap; }
    .manual-check-history__status { display: inline-block; border-radius: 999px; padding: .2rem .5rem; background: #e2e8f0; }
    .manual-check-history__status--succeeded { background: #dcfce7; color: #166534; }
    .manual-check-history__status--completedwitherrors { background: #fef3c7; color: #92400e; }
    .manual-check-history__status--failed { background: #fee2e2; color: #991b1b; }
    .manual-check-history__status--running { background: #dbeafe; color: #1e40af; }
    .manual-check-history__status--canceled { background: #fef3c7; color: #92400e; }
    .manual-check-history__outcome--match { color: #166534; }
    .manual-check-history__outcome--failed { color: #b91c1c; font-weight: 600; }
    .manual-check-history__trace-summary { min-width: 15rem; max-width: 24rem; }
    .manual-check-history__trace-summary small { display: block; margin-top: .35rem; overflow-wrap: anywhere; }
    .manual-check-history__callout { display: flex; flex-direction: column; align-items: flex-start; gap: .4rem; border: 1px solid #cbd5e1; border-radius: .375rem; background: #f8fafc; padding: .75rem; color: #334155; }
    .manual-check-history__callout--error { border-color: #fecaca; background: #fef2f2; color: #991b1b; }
    @media (max-width: 800px) {
      .manual-check-history { min-width: 0; }
    }
  `],
})
export class ManualCheckHistoryDialogComponent implements OnInit {
  readonly formatDuration = formatDuration;
  readonly data = inject<ManualCheckHistoryDialogData>(MAT_DIALOG_DATA);
  readonly dialogRef = inject<MatDialogRef<ManualCheckHistoryDialogComponent, number>>(MatDialogRef);
  private readonly dialog = inject(MatDialog);
  private readonly manualCheckService = inject(ManualCheckService);
  private readonly cdr = inject(ChangeDetectorRef);

  runs: ManualCheckRunSummary[] = [];
  loading = true;
  busyRunId: number | null = null;
  errorMessage = '';

  ngOnInit(): void {
    this.load();
  }

  load(): void {
    this.loading = true;
    this.errorMessage = '';
    this.manualCheckService.getRuns(this.data.gameId, 100)
      .pipe(finalize(() => {
        this.loading = false;
        this.cdr.markForCheck();
      }))
      .subscribe({
        next: (runs) => {
          this.runs = runs;
          this.cdr.markForCheck();
        },
        error: (error) => {
          this.errorMessage = this.getError(error, 'Unable to load automated-check history.');
          this.cdr.markForCheck();
        },
      });
  }

  statusLabel(run: ManualCheckRunSummary): string {
    return run.status === 'CompletedWithErrors' ? 'Completed with errors' : run.status;
  }

  openTrace(run: ManualCheckRunSummary): void {
    const initialView: ManualCheckTraceView = run.failedProducts > 0 || run.status === 'Failed'
      ? 'errors'
      : 'results';
    const data: ManualCheckTraceDialogData = { runId: run.id, initialView };
    this.dialog.open<ManualCheckTraceDialogComponent, ManualCheckTraceDialogData>(
      ManualCheckTraceDialogComponent,
      {
        width: 'min(68rem, 96vw)',
        maxWidth: '96vw',
        maxHeight: '92vh',
        data,
      },
    );
  }

  rerun(run: ManualCheckRunSummary): void {
    if (this.busyRunId !== null) {
      return;
    }
    this.busyRunId = run.id;
    this.errorMessage = '';
    this.manualCheckService.rerun(run.id)
      .pipe(finalize(() => {
        this.busyRunId = null;
        this.cdr.markForCheck();
      }))
      .subscribe({
        next: (accepted) => this.dialogRef.close(accepted.runId),
        error: (error) => {
          this.errorMessage = this.getError(error, 'Unable to rerun this check.');
          this.cdr.markForCheck();
        },
      });
  }

  private getError(error: unknown, fallback: string): string {
    if (error instanceof HttpErrorResponse) {
      const detail = error.error?.detail ?? error.error?.message;
      if (error.status === 429) {
        const retryAfter = error.headers.get('Retry-After');
        return detail ?? (retryAfter
          ? `Too many requests. Try again in ${retryAfter} seconds.`
          : 'Too many requests. Wait briefly and try again.');
      }
      return detail ?? fallback;
    }
    return error instanceof Error ? error.message : fallback;
  }
}
