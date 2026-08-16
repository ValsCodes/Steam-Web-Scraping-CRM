import { CommonModule } from '@angular/common';
import { HttpErrorResponse } from '@angular/common/http';
import {
  ChangeDetectionStrategy,
  ChangeDetectorRef,
  Component,
  inject,
  OnInit,
} from '@angular/core';
import {
  MAT_DIALOG_DATA,
  MatDialogModule,
  MatDialogRef,
} from '@angular/material/dialog';
import { MatButtonModule } from '@angular/material/button';
import { finalize } from 'rxjs';

import {
  ManualCheckRunDetail,
  ManualCheckRunSummary,
} from '../../models';
import { ManualCheckService } from '../../services';

export interface ManualCheckHistoryDialogData {
  gameId?: number;
}

type HistoryView = 'setup' | 'results' | 'errors';

@Component({
  selector: 'steam-manual-check-history-dialog',
  standalone: true,
  changeDetection: ChangeDetectionStrategy.OnPush,
  imports: [CommonModule, MatDialogModule, MatButtonModule],
  template: `
    <h2 mat-dialog-title>Automated Check History</h2>
    <mat-dialog-content class="manual-check-history">
      <p class="manual-check-history__intro">
        Every run keeps its setup, timestamps, correlation ID, matches, and product-level failure trace.
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
                    <button
                      mat-stroked-button
                      type="button"
                      (click)="openViewer(run, run.failedProducts > 0 || run.errorText ? 'errors' : 'results')">
                      View trace
                    </button>
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

      @if (viewerTitle) {
        <section class="manual-check-history__viewer" aria-live="polite">
          <div class="manual-check-history__viewer-heading">
            <div>
              <h3>{{ viewerTitle }}</h3>
              @if (viewerDetail) {
                <small>Correlation ID: {{ viewerDetail.correlationId }}</small>
              }
            </div>
            <button mat-button type="button" (click)="closeViewer()">Close details</button>
          </div>

          @if (viewerLoading) {
            <p>Loading run trace…</p>
          } @else if (viewerError) {
            <div class="manual-check-history__callout manual-check-history__callout--error" role="alert">
              {{ viewerError }}
            </div>
          } @else if (viewerDetail; as detail) {
            <nav class="manual-check-history__tabs" aria-label="Run trace sections">
              <button mat-stroked-button type="button" (click)="viewerView = 'setup'" [class.manual-check-history__tab--active]="viewerView === 'setup'">Setup</button>
              <button mat-stroked-button type="button" (click)="viewerView = 'results'" [class.manual-check-history__tab--active]="viewerView === 'results'">Matches ({{ detail.results.matches.length }})</button>
              <button mat-stroked-button type="button" (click)="viewerView = 'errors'" [class.manual-check-history__tab--active]="viewerView === 'errors'">Errors ({{ detail.results.errors.length }})</button>
            </nav>

            <dl class="manual-check-history__timeline">
              <div><dt>Requested</dt><dd>{{ detail.setup.requestedAtUtc | date:'medium' }}</dd></div>
              <div><dt>Started</dt><dd>{{ detail.startedAtUtc ? (detail.startedAtUtc | date:'medium') : 'Not started' }}</dd></div>
              <div><dt>Completed</dt><dd>{{ detail.completedAtUtc ? (detail.completedAtUtc | date:'medium') : 'Not completed' }}</dd></div>
            </dl>

            @if (viewerView === 'setup') {
              <div class="manual-check-history__panel">
                <h4>Preset snapshot</h4>
                <p><strong>{{ detail.setup.presetName }}</strong> · {{ detail.setup.matchMode === 'All' ? 'All criteria' : 'Any criterion' }}</p>
                <p>Top {{ detail.setup.listingLimit }} cheapest available listing(s) checked per product.</p>
                <p>{{ detail.setup.products.length }} product(s) captured from {{ detail.setup.gameUrlName || ('Source #' + detail.setup.gameUrlId) }}.</p>
                <ol class="manual-check-history__criteria">
                  @for (criterion of detail.setup.criteria; track $index) {
                    <li>
                      @if (criterion.nameContains) { <span>Name contains “{{ criterion.nameContains }}”</span> }
                      @if (criterion.nameContains && criterion.valueContains) { <span> and </span> }
                      @if (criterion.valueContains) { <span>Value contains “{{ criterion.valueContains }}”</span> }
                    </li>
                  }
                </ol>
              </div>
            } @else if (viewerView === 'results') {
              <div class="manual-check-history__panel">
                @if (detail.results.matches.length === 0) {
                  <p>No product assets matched this preset.</p>
                } @else {
                  @for (match of detail.results.matches; track match.productId) {
                    <article class="manual-check-history__record">
                      <h4>{{ match.productName }}</h4>
                      <p>{{ match.matchedAssets.length }} matched asset(s).</p>
                    </article>
                  }
                }
              </div>
            } @else {
              <div class="manual-check-history__panel">
                @if (detail.errorText) {
                  <div class="manual-check-history__callout manual-check-history__callout--error">
                    <strong>Run failure</strong>
                    <span>{{ detail.errorText }}</span>
                  </div>
                }

                @if (detail.results.errors.length === 0) {
                  <p>No product-level errors were recorded for this run.</p>
                } @else {
                  <div class="manual-check-history__error-list">
                    @for (failure of detail.results.errors; track failure.productId) {
                      <article class="manual-check-history__record manual-check-history__record--error">
                        <div class="manual-check-history__record-heading">
                          <h4>{{ failure.productName || ('Product #' + failure.productId) }}</h4>
                          <div class="manual-check-history__badges">
                            @if (failure.httpStatusCode) { <span>HTTP {{ failure.httpStatusCode }}</span> }
                            @if (failure.errorType) { <span>{{ failure.errorType }}</span> }
                          </div>
                        </div>
                        <p>{{ failure.error }}</p>
                        @if (failure.occurredAtUtc) { <small>{{ failure.occurredAtUtc | date:'medium' }}</small> }
                        <code>{{ failure.fullUrl }}</code>
                      </article>
                    }
                  </div>
                }
              </div>
            }
          }
        </section>
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
    .manual-check-history__table-wrap { overflow: auto; max-height: 55vh; border: 1px solid #e2e8f0; border-radius: .5rem; }
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
    .manual-check-history__viewer { margin-top: 1rem; border: 1px solid #cbd5e1; border-radius: .5rem; padding: 1rem; }
    .manual-check-history__viewer-heading, .manual-check-history__record-heading { display: flex; align-items: flex-start; justify-content: space-between; gap: 1rem; }
    .manual-check-history__viewer h3, .manual-check-history__viewer h4 { margin: 0; }
    .manual-check-history__tabs { display: flex; flex-wrap: wrap; gap: .5rem; margin: 1rem 0; }
    .manual-check-history__tab--active { background: #dbeafe; border-color: #93c5fd; }
    .manual-check-history__timeline { display: grid; grid-template-columns: repeat(3, minmax(0, 1fr)); gap: .75rem; margin: 0 0 1rem; }
    .manual-check-history__timeline div { border-radius: .375rem; background: #f8fafc; padding: .65rem; }
    .manual-check-history__timeline dt { color: #64748b; font-size: .75rem; text-transform: uppercase; }
    .manual-check-history__timeline dd { margin: .2rem 0 0; }
    .manual-check-history__panel { display: flex; flex-direction: column; gap: .75rem; }
    .manual-check-history__criteria { margin: 0; padding-left: 1.5rem; }
    .manual-check-history__error-list { display: grid; gap: .75rem; }
    .manual-check-history__record { border: 1px solid #e2e8f0; border-radius: .375rem; padding: .75rem; }
    .manual-check-history__record p { margin: .35rem 0; }
    .manual-check-history__record--error { border-color: #fecaca; background: #fffafa; }
    .manual-check-history__badges { display: flex; flex-wrap: wrap; justify-content: flex-end; gap: .35rem; }
    .manual-check-history__badges span { border-radius: 999px; background: #fee2e2; padding: .15rem .45rem; color: #991b1b; font-size: .75rem; }
    .manual-check-history__record code { display: block; margin-top: .5rem; overflow-wrap: anywhere; color: #475569; white-space: normal; }
    @media (max-width: 800px) {
      .manual-check-history__timeline { grid-template-columns: 1fr; }
      .manual-check-history__viewer-heading, .manual-check-history__record-heading { flex-direction: column; }
    }
  `],
})
export class ManualCheckHistoryDialogComponent implements OnInit {
  readonly data = inject<ManualCheckHistoryDialogData>(MAT_DIALOG_DATA);
  readonly dialogRef = inject<MatDialogRef<ManualCheckHistoryDialogComponent, number>>(MatDialogRef);
  private readonly manualCheckService = inject(ManualCheckService);
  private readonly cdr = inject(ChangeDetectorRef);

  runs: ManualCheckRunSummary[] = [];
  loading = true;
  busyRunId: number | null = null;
  errorMessage = '';
  viewerTitle = '';
  viewerView: HistoryView = 'results';
  viewerDetail: ManualCheckRunDetail | null = null;
  viewerLoading = false;
  viewerError = '';

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

  openViewer(run: ManualCheckRunSummary, view: HistoryView): void {
    this.viewerTitle = `Run #${run.id} · ${run.presetName}`;
    this.viewerView = view;
    this.viewerDetail = null;
    this.viewerError = '';
    this.viewerLoading = true;
    this.manualCheckService.getRun(run.id)
      .pipe(finalize(() => {
        this.viewerLoading = false;
        this.cdr.markForCheck();
      }))
      .subscribe({
        next: (detail) => {
          this.viewerDetail = detail;
          this.cdr.markForCheck();
        },
        error: (error) => {
          this.viewerError = this.getError(error, 'Unable to load the run trace.');
          this.cdr.markForCheck();
        },
      });
  }

  closeViewer(): void {
    this.viewerTitle = '';
    this.viewerDetail = null;
    this.viewerError = '';
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
