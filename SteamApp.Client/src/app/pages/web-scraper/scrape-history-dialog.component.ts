import { CommonModule } from '@angular/common';
import {
  ChangeDetectorRef,
  Component,
  DestroyRef,
  OnInit,
  inject,
} from '@angular/core';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';

import { MatButtonModule } from '@angular/material/button';
import {
  MAT_DIALOG_DATA,
  MatDialog,
  MatDialogModule,
  MatDialogRef,
} from '@angular/material/dialog';
import { MatIconModule } from '@angular/material/icon';
import { MatTableModule } from '@angular/material/table';
import { MatTooltipModule } from '@angular/material/tooltip';

import {
  ScrapeHistory,
  ScrapeHistoryRerunResponse,
} from '../../models';
import { SteamService } from '../../services';

@Component({
  selector: 'steam-scrape-history-json-dialog',
  standalone: true,
  imports: [CommonModule, MatButtonModule, MatDialogModule, MatIconModule],
  template: `
    <div class="history-detail-dialog">
      <div class="history-detail-dialog__header">
        <div>
          <h2>{{ data.title }}</h2>
          <p>{{ data.subtitle }}</p>
        </div>

        <button mat-icon-button type="button" (click)="close()" aria-label="Close dialog">
          <mat-icon>close</mat-icon>
        </button>
      </div>

      <pre>{{ data.content }}</pre>

      <div class="history-detail-dialog__actions">
        <button mat-flat-button color="primary" type="button" (click)="close()">OK</button>
      </div>
    </div>
  `,
  styles: [`
    .history-detail-dialog {
      display: flex;
      max-height: 82vh;
      width: min(840px, 92vw);
      flex-direction: column;
      gap: 16px;
      padding: 20px;
    }

    .history-detail-dialog__header {
      display: flex;
      align-items: flex-start;
      justify-content: space-between;
      gap: 16px;
    }

    .history-detail-dialog h2 {
      margin: 0;
      color: #1f2937;
      font-size: 20px;
      font-weight: 600;
      line-height: 1.25;
    }

    .history-detail-dialog p {
      margin: 4px 0 0;
      color: #64748b;
      font-size: 14px;
    }

    .history-detail-dialog pre {
      min-height: 220px;
      max-height: 56vh;
      overflow: auto;
      border: 1px solid #e2e8f0;
      border-radius: 6px;
      background: #0f172a;
      color: #e2e8f0;
      font-size: 13px;
      line-height: 1.5;
      margin: 0;
      padding: 16px;
      white-space: pre-wrap;
      word-break: break-word;
    }

    .history-detail-dialog__actions {
      display: flex;
      justify-content: flex-end;
    }
  `],
})
export class ScrapeHistoryJsonDialogComponent {
  private readonly dialogRef = inject(MatDialogRef<ScrapeHistoryJsonDialogComponent>);
  public readonly data = inject<{ title: string; subtitle: string; content: string }>(MAT_DIALOG_DATA);

  public close(): void {
    this.dialogRef.close();
  }
}

@Component({
  selector: 'steam-scrape-history-dialog',
  standalone: true,
  imports: [
    CommonModule,
    MatButtonModule,
    MatDialogModule,
    MatIconModule,
    MatTableModule,
    MatTooltipModule,
  ],
  template: `
    <div class="scrape-history-dialog">
      <div class="scrape-history-dialog__header">
        <div>
          <h2>Automated Scrape History</h2>
          <p>Review stored scrape setup, results, errors, and rerun prior setups.</p>
        </div>

        <button mat-icon-button type="button" (click)="close()" aria-label="Close history dialog">
          <mat-icon>close</mat-icon>
        </button>
      </div>

      @if (isLoading) {
      <div class="scrape-history-dialog__state">Loading history...</div>
      } @else if (loadError) {
      <div class="scrape-history-dialog__state scrape-history-dialog__state--error">
        <span>{{ loadError }}</span>
        <button mat-flat-button color="primary" type="button" (click)="loadHistory()">
          Retry
        </button>
      </div>
      } @else if (history.length === 0) {
      <div class="scrape-history-dialog__state">No scrape history has been recorded yet.</div>
      } @else {
      <div class="scrape-history-dialog__table-wrap">
        <table mat-table [dataSource]="history" class="app-data-table scrape-history-dialog__table">
          <ng-container matColumnDef="date">
            <th mat-header-cell *matHeaderCellDef>Date</th>
            <td mat-cell *matCellDef="let row">{{ row.date | date:'medium' }}</td>
          </ng-container>

          <ng-container matColumnDef="scrapeType">
            <th mat-header-cell *matHeaderCellDef>Type</th>
            <td mat-cell *matCellDef="let row">{{ row.scrapeType }}</td>
          </ng-container>

          <ng-container matColumnDef="gameUrl">
            <th mat-header-cell *matHeaderCellDef>Game URL</th>
            <td mat-cell *matCellDef="let row">{{ getGameUrlLabel(row) }}</td>
          </ng-container>

          <ng-container matColumnDef="page">
            <th mat-header-cell *matHeaderCellDef>Page</th>
            <td mat-cell *matCellDef="let row">{{ row.page }}</td>
          </ng-container>

          <ng-container matColumnDef="resultCount">
            <th mat-header-cell *matHeaderCellDef>Results</th>
            <td mat-cell *matCellDef="let row">{{ row.resultCount }}</td>
          </ng-container>

          <ng-container matColumnDef="status">
            <th mat-header-cell *matHeaderCellDef>Status</th>
            <td mat-cell *matCellDef="let row">
              <span class="scrape-history-dialog__status"
                [class.scrape-history-dialog__status--error]="row.isHaveError">
                {{ row.isHaveError ? 'Error' : 'Success' }}
              </span>
            </td>
          </ng-container>

          <ng-container matColumnDef="actions">
            <th mat-header-cell *matHeaderCellDef>Actions</th>
            <td mat-cell *matCellDef="let row">
              <div class="scrape-history-dialog__actions">
                <button mat-icon-button type="button" matTooltip="Rerun setup"
                  (click)="rerun(row)"
                  [disabled]="rerunningId === row.id || detailLoadingId === row.id"
                  aria-label="Rerun scrape setup">
                  <mat-icon>replay</mat-icon>
                </button>

                <button mat-icon-button type="button" matTooltip="View setup data"
                  (click)="openSetup(row)"
                  [disabled]="detailLoadingId === row.id"
                  aria-label="View setup data">
                  <mat-icon>settings</mat-icon>
                </button>

                <button mat-icon-button type="button" matTooltip="View results"
                  (click)="openResults(row)"
                  [disabled]="detailLoadingId === row.id"
                  aria-label="View scrape results">
                  <mat-icon>data_object</mat-icon>
                </button>

                <button mat-icon-button type="button" matTooltip="View error"
                  (click)="openError(row)"
                  [disabled]="!row.isHaveError || detailLoadingId === row.id"
                  aria-label="View scrape error">
                  <mat-icon>error_outline</mat-icon>
                </button>
              </div>
            </td>
          </ng-container>

          <tr mat-header-row *matHeaderRowDef="displayedColumns"></tr>
          <tr mat-row *matRowDef="let row; columns: displayedColumns;"></tr>
        </table>
      </div>
      }
    </div>
  `,
  styles: [`
    .scrape-history-dialog {
      display: flex;
      max-height: 86vh;
      width: min(1120px, 96vw);
      flex-direction: column;
      gap: 16px;
      padding: 20px;
    }

    .scrape-history-dialog__header {
      display: flex;
      align-items: flex-start;
      justify-content: space-between;
      gap: 16px;
    }

    .scrape-history-dialog h2 {
      margin: 0;
      color: #1f2937;
      font-size: 20px;
      font-weight: 600;
      line-height: 1.25;
    }

    .scrape-history-dialog p {
      margin: 4px 0 0;
      color: #64748b;
      font-size: 14px;
    }

    .scrape-history-dialog__state {
      display: flex;
      min-height: 220px;
      align-items: center;
      justify-content: center;
      flex-direction: column;
      gap: 12px;
      border: 1px solid #e2e8f0;
      border-radius: 6px;
      color: #475569;
      font-size: 14px;
    }

    .scrape-history-dialog__state--error {
      border-color: #fecaca;
      background: #fef2f2;
      color: #991b1b;
    }

    .scrape-history-dialog__table-wrap {
      overflow: auto;
      border: 1px solid #e2e8f0;
      border-radius: 6px;
    }

    .scrape-history-dialog__table {
      min-width: 880px;
    }

    .scrape-history-dialog__actions {
      display: flex;
      align-items: center;
      gap: 4px;
    }

    .scrape-history-dialog__status {
      display: inline-flex;
      align-items: center;
      border-radius: 999px;
      background: #dcfce7;
      color: #166534;
      font-size: 12px;
      font-weight: 600;
      line-height: 1;
      padding: 6px 10px;
    }

    .scrape-history-dialog__status--error {
      background: #fee2e2;
      color: #991b1b;
    }
  `],
})
export class ScrapeHistoryDialogComponent implements OnInit {
  private readonly steamService = inject(SteamService);
  private readonly dialog = inject(MatDialog);
  private readonly destroyRef = inject(DestroyRef);
  private readonly cdr = inject(ChangeDetectorRef);

  public readonly displayedColumns = [
    'date',
    'scrapeType',
    'gameUrl',
    'page',
    'resultCount',
    'status',
    'actions',
  ];

  public history: ScrapeHistory[] = [];
  public isLoading = true;
  public loadError: string | null = null;
  public rerunningId: number | null = null;
  public detailLoadingId: number | null = null;

  public constructor(
    private readonly dialogRef: MatDialogRef<
      ScrapeHistoryDialogComponent,
      ScrapeHistoryRerunResponse | undefined
    >,
  ) {}

  public ngOnInit(): void {
    setTimeout(() => {
      if (!this.destroyRef.destroyed) {
        this.loadHistory();
      }
    }, 0);
  }

  public close(): void {
    this.dialogRef.close();
  }

  public getGameUrlLabel(row: ScrapeHistory): string {
    return row.gameUrlName?.trim() || `Game URL #${row.gameUrlId}`;
  }

  public rerun(row: ScrapeHistory): void {
    this.rerunningId = row.id;
    this.cdr.markForCheck();

    this.steamService
      .rerunScrapeHistory(row.id)
      .pipe(
        takeUntilDestroyed(this.destroyRef),
      )
      .subscribe({
        next: (response) => {
          this.rerunningId = null;
          this.dialogRef.close(response);
          this.cdr.markForCheck();
        },
        error: () => {
          this.rerunningId = null;
          this.loadError = 'Rerun failed. Try again or inspect the latest error details.';
          this.cdr.markForCheck();
        },
      });
  }

  public openSetup(row: ScrapeHistory): void {
    this.openDetail(row, 'Setup Data', (detail) => detail.setupJson);
  }

  public openResults(row: ScrapeHistory): void {
    this.openDetail(row, 'Scrape Results', (detail) => detail.resultsJson);
  }

  public openError(row: ScrapeHistory): void {
    if (!row.isHaveError) {
      return;
    }

    this.openDetail(row, 'Scrape Error', (detail) => detail.errorText);
  }

  public loadHistory(): void {
    this.isLoading = true;
    this.loadError = null;
    this.cdr.markForCheck();

    this.steamService
      .getScrapeHistory()
      .pipe(
        takeUntilDestroyed(this.destroyRef),
      )
      .subscribe({
        next: (history) => {
          this.history = history;
          this.isLoading = false;
          this.cdr.detectChanges();
        },
        error: () => {
          this.history = [];
          this.isLoading = false;
          this.loadError = 'History could not be loaded. Check the API response and try again.';
          this.cdr.detectChanges();
        },
      });
  }

  private openDetail(
    row: ScrapeHistory,
    title: string,
    selectContent: (detail: { setupJson?: string | null; resultsJson?: string | null; errorText?: string | null }) => string | null | undefined,
  ): void {
    this.detailLoadingId = row.id;
    this.loadError = null;
    this.cdr.markForCheck();

    this.steamService
      .getScrapeHistoryDetail(row.id)
      .pipe(
        takeUntilDestroyed(this.destroyRef),
      )
      .subscribe({
        next: (detail) => {
          this.detailLoadingId = null;
          this.dialog.open(ScrapeHistoryJsonDialogComponent, {
            data: {
              title,
              subtitle: `${detail.scrapeType} on page ${detail.page}`,
              content: this.formatContent(selectContent(detail)),
            },
          });
          this.cdr.markForCheck();
        },
        error: () => {
          this.detailLoadingId = null;
          this.loadError = 'History details could not be loaded.';
          this.cdr.markForCheck();
        },
      });
  }

  private formatContent(content: string | null | undefined): string {
    if (!content) {
      return 'No data recorded.';
    }

    try {
      return JSON.stringify(JSON.parse(content), null, 2);
    } catch {
      return content;
    }
  }
}
