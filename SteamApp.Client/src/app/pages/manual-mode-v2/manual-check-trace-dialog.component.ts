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
import { MatIconModule } from '@angular/material/icon';
import { MatMenuModule } from '@angular/material/menu';
import { finalize } from 'rxjs';

import { formatDuration } from '../../common';
import {
  ManualCheckProductTrace,
  ManualCheckRunDetail,
} from '../../models';
import { ManualCheckService } from '../../services';
import { ManualCheckSteamResultDialogComponent } from './manual-check-steam-result-dialog.component';

export type ManualCheckTraceView = 'setup' | 'results' | 'errors';

export interface ManualCheckTraceDialogData {
  runId?: number;
  detail?: ManualCheckRunDetail;
  initialView?: ManualCheckTraceView;
}

@Component({
  selector: 'steam-manual-check-trace-dialog',
  standalone: true,
  changeDetection: ChangeDetectionStrategy.OnPush,
  imports: [CommonModule, MatDialogModule, MatButtonModule, MatIconModule, MatMenuModule],
  template: `
    <h2 mat-dialog-title>{{ title }}</h2>
    <mat-dialog-content class="manual-check-trace">
      @if (detail) {
        <p class="manual-check-trace__intro">
          {{ detail.presetName }} · Correlation ID: {{ detail.correlationId }}
        </p>
      }

      @if (loading) {
        <div class="manual-check-trace__loading" role="status">Loading run trace…</div>
      } @else if (errorMessage) {
        <div class="manual-check-trace__callout manual-check-trace__callout--error" role="alert">
          <strong>Run trace could not be loaded</strong>
          <span>{{ errorMessage }}</span>
          @if (data.runId !== undefined) {
            <button mat-stroked-button type="button" (click)="load(data.runId)">Try again</button>
          }
        </div>
      } @else if (detail; as run) {
        <nav class="manual-check-trace__tabs" aria-label="Run trace sections">
          <button mat-stroked-button type="button" (click)="view = 'setup'" [class.manual-check-trace__tab--active]="view === 'setup'">Setup</button>
          <button mat-stroked-button type="button" (click)="view = 'results'" [class.manual-check-trace__tab--active]="view === 'results'">Checks ({{ run.results.productTraces.length }})</button>
          <button mat-stroked-button type="button" (click)="view = 'errors'" [class.manual-check-trace__tab--active]="view === 'errors'">Errors ({{ run.results.errors.length }})</button>
        </nav>

        <dl class="manual-check-trace__timeline">
          <div><dt>Requested</dt><dd>{{ run.setup.requestedAtUtc | date:'medium' }}</dd></div>
          <div><dt>Started</dt><dd>{{ run.startedAtUtc ? (run.startedAtUtc | date:'medium') : 'Not started' }}</dd></div>
          <div><dt>Completed</dt><dd>{{ run.completedAtUtc ? (run.completedAtUtc | date:'medium') : 'Not completed' }}</dd></div>
          <div><dt>Duration</dt><dd>{{ formatDuration(run.durationMilliseconds) }}</dd></div>
        </dl>

        @if (view === 'setup') {
          <div class="manual-check-trace__panel">
            <h3>Preset snapshot</h3>
            <p><strong>{{ run.setup.presetName }}</strong> · Criteria are evaluated from top to bottom.</p>
            <p>Top {{ run.setup.listingLimit }} cheapest available listing(s) checked per product.</p>
            <p>
              Cooldown between checks:
              @if (run.setup.cooldownMinutes === null || run.setup.cooldownSeconds === null) {
                <span>server default</span>
              } @else {
                <span>{{ run.setup.cooldownMinutes }}m {{ run.setup.cooldownSeconds }}s</span>
              }
            </p>
            <p>Steam data: {{ run.setup.bypassCache ? 'fresh fetch requested; cache bypassed' : '20-minute cache allowed' }}.</p>
            <p>{{ run.setup.products.length }} product(s) captured from {{ run.setup.gameUrlName || ('Source #' + run.setup.gameUrlId) }}.</p>
            <ol class="manual-check-trace__criteria">
              @for (criterion of run.setup.criteria; track $index; let i = $index) {
                <li>
                  <strong>{{ i === 0 ? 'Start' : criterion.conditionOperatorName }}</strong>
                  <span> · </span>
                  @if (criterion.nameContains) { <span>Name contains “{{ criterion.nameContains }}”</span> }
                  @if (criterion.nameContains && criterion.valueContains) { <span> and </span> }
                  @if (criterion.valueContains) { <span>Value contains “{{ criterion.valueContains }}”</span> }
                </li>
              }
            </ol>
          </div>
        } @else if (view === 'results') {
          <div class="manual-check-trace__panel">
            @if (run.results.productTraces.length === 0) {
              <div class="manual-check-trace__callout">
                No parsed Steam API result was stored. The product request may have failed before parsing, or this run predates Steam result tracing.
              </div>
              @for (match of run.results.matches; track match.productId) {
                <article class="manual-check-trace__record">
                  <h3>{{ match.productName }}</h3>
                  <p>{{ match.matchedAssets.length }} matched asset(s). Steam result trace unavailable.</p>
                </article>
              }
            } @else {
              <p class="manual-check-trace__muted">
                Every attempted product keeps a duration trace. Parsed Steam results are available when the request reached that stage.
              </p>
              <div class="manual-check-trace__product-results-wrap">
                <table>
                  <thead>
                    <tr>
                      <th>Product</th>
                      <th>Match result</th>
                      <th>Matched assets</th>
                      <th>Duration</th>
                      <th>Actions</th>
                    </tr>
                  </thead>
                  <tbody>
                    @for (trace of run.results.productTraces; track trace.productId) {
                      <tr>
                        <td>
                          <strong>{{ trace.productName || ('Product #' + trace.productId) }}</strong><br />
                          <small>Product #{{ trace.productId }}</small>
                        </td>
                        <td>
                          <span
                            class="manual-check-trace__result-status"
                            [class.manual-check-trace__result-status--failed]="!trace.matchEvaluated"
                            [class.manual-check-trace__result-status--matched]="trace.matchEvaluated && trace.matched">
                            {{ !trace.matchEvaluated ? 'Check failed' : trace.matched ? 'Matched' : 'No match' }}
                          </span>
                        </td>
                        <td>{{ trace.matchedAssetCount }}</td>
                        <td>{{ formatDuration(trace.durationMilliseconds) }}</td>
                        <td>
                          @if (trace.steamApiResultJson) {
                            <div class="table-actions-cell">
                              <button
                                type="button"
                                mat-icon-button
                                class="table-actions-trigger"
                                [matMenuTriggerFor]="steamResultActionsMenu"
                                [attr.aria-label]="'Open Steam result actions for ' + (trace.productName || ('product ' + trace.productId))">
                                <mat-icon>more_horiz</mat-icon>
                              </button>
                              <mat-menu #steamResultActionsMenu="matMenu" xPosition="before" panelClass="table-actions-menu">
                                <button type="button" mat-menu-item (click)="openSteamResult(trace)">
                                  <span>View Steam API result</span>
                                </button>
                              </mat-menu>
                            </div>
                          } @else {
                            <span class="manual-check-trace__muted">No response</span>
                          }
                        </td>
                      </tr>
                    }
                  </tbody>
                </table>
              </div>
            }
          </div>
        } @else {
          <div class="manual-check-trace__panel">
            @if (run.errorText) {
              <div class="manual-check-trace__callout manual-check-trace__callout--error">
                <strong>Run failure</strong>
                <span>{{ run.errorText }}</span>
              </div>
            }

            @if (run.results.errors.length === 0) {
              <p>No product-level errors were recorded for this run.</p>
            } @else {
              <div class="manual-check-trace__error-list">
                @for (failure of run.results.errors; track failure.productId) {
                  <article class="manual-check-trace__record manual-check-trace__record--error">
                    <div class="manual-check-trace__record-heading">
                      <h3>{{ failure.productName || ('Product #' + failure.productId) }}</h3>
                      <div class="manual-check-trace__badges">
                        @if (failure.httpStatusCode) { <span>HTTP {{ failure.httpStatusCode }}</span> }
                        @if (failure.errorType) { <span>{{ failure.errorType }}</span> }
                      </div>
                    </div>
                    <p>{{ failure.error }}</p>
                    @if (failure.occurredAtUtc) { <small>{{ failure.occurredAtUtc | date:'medium' }}</small> }
                    <small>Duration: {{ formatDuration(productDuration(run, failure.productId)) }}</small>
                    <code>{{ failure.fullUrl }}</code>
                  </article>
                }
              </div>
            }
          </div>
        }
      }
    </mat-dialog-content>
    <mat-dialog-actions align="end">
      <button mat-flat-button type="button" (click)="dialogRef.close()">Close</button>
    </mat-dialog-actions>
  `,
  styles: [`
    .manual-check-trace { min-width: min(64rem, 90vw); }
    .manual-check-trace__intro { margin: 0 0 1rem; color: #64748b; overflow-wrap: anywhere; }
    .manual-check-trace__loading { display: flex; min-height: 12rem; align-items: center; justify-content: center; color: #64748b; }
    .manual-check-trace__tabs { display: flex; flex-wrap: wrap; gap: .5rem; margin: 0 0 1rem; }
    .manual-check-trace__tab--active { background: #dbeafe; border-color: #93c5fd; }
    .manual-check-trace__timeline { display: grid; grid-template-columns: repeat(4, minmax(0, 1fr)); gap: .75rem; margin: 0 0 1rem; }
    .manual-check-trace__timeline div { border-radius: .375rem; background: #f8fafc; padding: .65rem; }
    .manual-check-trace__timeline dt { color: #64748b; font-size: .75rem; text-transform: uppercase; }
    .manual-check-trace__timeline dd { margin: .2rem 0 0; }
    .manual-check-trace__panel { display: flex; flex-direction: column; gap: .75rem; }
    .manual-check-trace__panel h3, .manual-check-trace__record h3 { margin: 0; }
    .manual-check-trace__callout { display: flex; flex-direction: column; align-items: flex-start; gap: .4rem; border: 1px solid #cbd5e1; border-radius: .375rem; background: #f8fafc; padding: .75rem; color: #334155; }
    .manual-check-trace__callout--error { border-color: #fecaca; background: #fef2f2; color: #991b1b; }
    .manual-check-trace__product-results-wrap { overflow: auto; border: 1px solid #e2e8f0; border-radius: .5rem; }
    table { width: 100%; border-collapse: collapse; font-size: .875rem; }
    th, td { border-bottom: 1px solid #e2e8f0; padding: .7rem; text-align: left; vertical-align: top; }
    th { color: #475569; position: sticky; top: 0; z-index: 1; background: #f8fafc; }
    tbody tr:last-child td { border-bottom: 0; }
    small, .manual-check-trace__muted { color: #64748b; }
    .manual-check-trace__result-status { display: inline-block; border-radius: 999px; background: #e2e8f0; padding: .2rem .5rem; color: #475569; }
    .manual-check-trace__result-status--matched { background: #dcfce7; color: #166534; }
    .manual-check-trace__result-status--failed { background: #fee2e2; color: #991b1b; }
    .table-actions-cell { display: flex; justify-content: center; }
    .manual-check-trace__criteria { margin: 0; padding-left: 1.5rem; }
    .manual-check-trace__error-list { display: grid; gap: .75rem; }
    .manual-check-trace__record { border: 1px solid #e2e8f0; border-radius: .375rem; padding: .75rem; }
    .manual-check-trace__record p { margin: .35rem 0; }
    .manual-check-trace__record--error { border-color: #fecaca; background: #fffafa; }
    .manual-check-trace__record-heading { display: flex; align-items: flex-start; justify-content: space-between; gap: 1rem; }
    .manual-check-trace__badges { display: flex; flex-wrap: wrap; justify-content: flex-end; gap: .35rem; }
    .manual-check-trace__badges span { border-radius: 999px; background: #fee2e2; padding: .15rem .45rem; color: #991b1b; font-size: .75rem; }
    .manual-check-trace__record small { display: block; margin-top: .35rem; }
    .manual-check-trace__record code { display: block; margin-top: .5rem; overflow-wrap: anywhere; color: #475569; white-space: normal; }
    @media (max-width: 800px) {
      .manual-check-trace { min-width: 0; }
      .manual-check-trace__timeline { grid-template-columns: 1fr; }
      .manual-check-trace__record-heading { flex-direction: column; }
    }
  `],
})
export class ManualCheckTraceDialogComponent implements OnInit {
  readonly formatDuration = formatDuration;
  readonly data = inject<ManualCheckTraceDialogData>(MAT_DIALOG_DATA);
  readonly dialogRef = inject<MatDialogRef<ManualCheckTraceDialogComponent>>(MatDialogRef);
  private readonly dialog = inject(MatDialog);
  private readonly manualCheckService = inject(ManualCheckService);
  private readonly cdr = inject(ChangeDetectorRef);

  detail: ManualCheckRunDetail | null = null;
  view: ManualCheckTraceView = 'results';
  loading = false;
  errorMessage = '';

  get title(): string {
    const runId = this.detail?.id ?? this.data.runId;
    return runId === undefined
      ? 'Automated Check Result'
      : `Automated Check Result · Run #${runId}`;
  }

  ngOnInit(): void {
    this.view = this.data.initialView ?? 'results';
    if (this.data.detail) {
      this.detail = this.data.detail;
      return;
    }

    if (this.data.runId === undefined) {
      this.errorMessage = 'No automated-check run was selected.';
      return;
    }

    this.load(this.data.runId);
  }

  load(runId: number): void {
    this.loading = true;
    this.errorMessage = '';
    this.manualCheckService.getRun(runId)
      .pipe(finalize(() => {
        this.loading = false;
        this.cdr.markForCheck();
      }))
      .subscribe({
        next: (detail) => {
          this.detail = detail;
          this.cdr.markForCheck();
        },
        error: (error) => {
          this.errorMessage = this.getError(error, 'Unable to load the run trace.');
          this.cdr.markForCheck();
        },
      });
  }

  openSteamResult(trace: ManualCheckProductTrace): void {
    this.dialog.open(ManualCheckSteamResultDialogComponent, {
      width: '64rem',
      maxWidth: '96vw',
      maxHeight: '90vh',
      data: trace,
    });
  }

  productDuration(detail: ManualCheckRunDetail, productId: number): number | null {
    return detail.results.productTraces.find((trace) => trace.productId === productId)
      ?.durationMilliseconds ?? null;
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
