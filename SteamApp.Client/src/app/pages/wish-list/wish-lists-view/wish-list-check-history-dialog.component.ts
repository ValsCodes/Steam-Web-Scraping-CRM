import { CommonModule } from '@angular/common';
import { Component, DestroyRef, inject, signal } from '@angular/core';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { finalize } from 'rxjs';

import { MAT_DIALOG_DATA, MatDialogModule, MatDialogRef } from '@angular/material/dialog';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatPaginatorModule, PageEvent } from '@angular/material/paginator';
import { MatTableModule } from '@angular/material/table';

import { WishListCheckHistory } from '../../../models/wish-list.model';
import { WishListService } from '../../../services/wish-list/wish-list.service';

export interface WishListCheckHistoryDialogData {
  wishListId: number;
  alertName: string;
  gameName: string;
}

@Component({
  selector: 'steam-wish-list-check-history-dialog',
  standalone: true,
  imports: [
    CommonModule,
    MatButtonModule,
    MatDialogModule,
    MatIconModule,
    MatPaginatorModule,
    MatTableModule,
  ],
  template: `
    <div class="price-check-history">
      <header class="price-check-history__header">
        <div>
          <h2>Price Check History</h2>
          <p>{{ data.alertName }} · {{ data.gameName }}</p>
        </div>
        <button mat-icon-button type="button" (click)="close()" aria-label="Close price check history">
          <mat-icon>close</mat-icon>
        </button>
      </header>

      @if (isLoading()) {
        <div class="price-check-history__state" role="status">Loading previous checks…</div>
      } @else if (loadError()) {
        <div class="price-check-history__state price-check-history__state--error" role="alert">
          <span>{{ loadError() }}</span>
          <button mat-stroked-button type="button" (click)="loadHistory()">Retry</button>
        </div>
      } @else if (items().length === 0) {
        <div class="price-check-history__state">
          No checks have been recorded for this price alert yet.
        </div>
      } @else {
        <div class="price-check-history__table-wrap">
          <table mat-table [dataSource]="items()" class="app-data-table">
            <ng-container matColumnDef="checkedAt">
              <th mat-header-cell *matHeaderCellDef>Checked</th>
              <td mat-cell *matCellDef="let row">{{ row.startedAtUtc | date:'medium' }}</td>
            </ng-container>

            <ng-container matColumnDef="source">
              <th mat-header-cell *matHeaderCellDef>Source</th>
              <td mat-cell *matCellDef="let row">{{ row.source }}</td>
            </ng-container>

            <ng-container matColumnDef="targetPrice">
              <th mat-header-cell *matHeaderCellDef>Target</th>
              <td mat-cell *matCellDef="let row">{{ formatPrice(row.targetPrice) }}</td>
            </ng-container>

            <ng-container matColumnDef="currentPrice">
              <th mat-header-cell *matHeaderCellDef>Current</th>
              <td mat-cell *matCellDef="let row">{{ formatPrice(row.currentPrice) }}</td>
            </ng-container>

            <ng-container matColumnDef="outcome">
              <th mat-header-cell *matHeaderCellDef>Outcome</th>
              <td mat-cell *matCellDef="let row">{{ outcomeLabel(row) }}</td>
            </ng-container>

            <ng-container matColumnDef="status">
              <th mat-header-cell *matHeaderCellDef>Status</th>
              <td mat-cell *matCellDef="let row">
                <span [class]="'price-check-history__status price-check-history__status--' + row.status.toLowerCase()">
                  {{ row.status }}
                </span>
              </td>
            </ng-container>

            <ng-container matColumnDef="duration">
              <th mat-header-cell *matHeaderCellDef>Duration</th>
              <td mat-cell *matCellDef="let row">{{ formatDuration(row.durationMilliseconds) }}</td>
            </ng-container>

            <ng-container matColumnDef="trace">
              <th mat-header-cell *matHeaderCellDef>Trace</th>
              <td mat-cell *matCellDef="let row" class="price-check-history__trace">
                <code>{{ row.correlationId }}</code>
                @if (row.errorCode) {
                  <span class="price-check-history__error-code">{{ row.errorCode }}</span>
                }
                @if (row.errorText) {
                  <span>{{ row.errorText }}</span>
                }
              </td>
            </ng-container>

            <tr mat-header-row *matHeaderRowDef="displayedColumns"></tr>
            <tr mat-row *matRowDef="let row; columns: displayedColumns"
              [class.price-check-history__row--failed]="row.status === 'Failed' || row.status === 'Canceled'">
            </tr>
          </table>
        </div>

        <mat-paginator
          [length]="totalCount()"
          [pageIndex]="pageNumber() - 1"
          [pageSize]="pageSize()"
          [pageSizeOptions]="[10, 25, 50, 100]"
          showFirstLastButtons
          (page)="pageChanged($event)">
        </mat-paginator>
      }

      <footer>
        <button mat-flat-button type="button" (click)="close()">Close</button>
      </footer>
    </div>
  `,
  styles: [`
    .price-check-history { width: min(76rem, 94vw); max-height: 88vh; display: flex; flex-direction: column; padding: 1.25rem; }
    .price-check-history__header { display: flex; align-items: flex-start; justify-content: space-between; gap: 1rem; }
    .price-check-history__header h2 { margin: 0; color: #1f2937; font-size: 1.25rem; font-weight: 600; }
    .price-check-history__header p { margin: .25rem 0 0; color: #64748b; }
    .price-check-history__state { min-height: 12rem; display: flex; flex-direction: column; align-items: center; justify-content: center; gap: .75rem; color: #64748b; text-align: center; }
    .price-check-history__state--error { color: #991b1b; }
    .price-check-history__table-wrap { margin-top: 1rem; max-height: 58vh; overflow: auto; border: 1px solid #e2e8f0; border-radius: .5rem; }
    .price-check-history__trace { min-width: 14rem; max-width: 22rem; }
    .price-check-history__trace code, .price-check-history__trace span { display: block; overflow-wrap: anywhere; }
    .price-check-history__trace span { margin-top: .25rem; color: #991b1b; }
    .price-check-history__trace .price-check-history__error-code { font-weight: 600; }
    .price-check-history__status { display: inline-block; border-radius: 999px; padding: .2rem .5rem; background: #e2e8f0; }
    .price-check-history__status--succeeded { background: #dcfce7; color: #166534; }
    .price-check-history__status--failed { background: #fee2e2; color: #991b1b; }
    .price-check-history__status--canceled { background: #fef3c7; color: #92400e; }
    .price-check-history__status--running { background: #dbeafe; color: #1e40af; }
    .price-check-history__row--failed { background: #fffafa; }
    footer { display: flex; justify-content: flex-end; padding-top: 1rem; }
    @media (max-width: 800px) { .price-check-history { width: auto; } }
  `],
})
export class WishListCheckHistoryDialogComponent {
  private readonly dialogRef = inject(MatDialogRef<WishListCheckHistoryDialogComponent>);
  private readonly wishListService = inject(WishListService);
  private readonly destroyRef = inject(DestroyRef);

  readonly data = inject<WishListCheckHistoryDialogData>(MAT_DIALOG_DATA);
  readonly items = signal<WishListCheckHistory[]>([]);
  readonly isLoading = signal(true);
  readonly loadError = signal('');
  readonly pageNumber = signal(1);
  readonly pageSize = signal(25);
  readonly totalCount = signal(0);
  readonly displayedColumns = [
    'checkedAt',
    'source',
    'targetPrice',
    'currentPrice',
    'outcome',
    'status',
    'duration',
    'trace',
  ];

  private readonly euroPriceFormatter = new Intl.NumberFormat('en-US', {
    style: 'currency',
    currency: 'EUR',
    minimumFractionDigits: 2,
    maximumFractionDigits: 2,
  });

  constructor() {
    this.loadHistory();
  }

  loadHistory(): void {
    this.isLoading.set(true);
    this.loadError.set('');

    this.wishListService
      .getCheckHistory(this.data.wishListId, this.pageNumber(), this.pageSize())
      .pipe(
        takeUntilDestroyed(this.destroyRef),
        finalize(() => this.isLoading.set(false)),
      )
      .subscribe({
        next: (page) => {
          this.items.set(page.items);
          this.pageNumber.set(page.pageNumber);
          this.pageSize.set(page.pageSize);
          this.totalCount.set(page.totalCount);
        },
        error: () => {
          this.items.set([]);
          this.loadError.set('Unable to load price-check history. Please try again.');
        },
      });
  }

  pageChanged(event: PageEvent): void {
    this.pageNumber.set(event.pageIndex + 1);
    this.pageSize.set(event.pageSize);
    this.loadHistory();
  }

  formatPrice(price: number | null): string {
    if (price === null) {
      return 'Unavailable';
    }

    return price === 0 ? 'Free' : this.euroPriceFormatter.format(price);
  }

  formatDuration(durationMilliseconds: number | null): string {
    if (durationMilliseconds === null) {
      return 'Unavailable';
    }

    return durationMilliseconds < 1000
      ? `${durationMilliseconds} ms`
      : `${(durationMilliseconds / 1000).toFixed(1)} s`;
  }

  outcomeLabel(trace: WishListCheckHistory): string {
    if (trace.status === 'Failed') {
      return 'Failed';
    }
    if (trace.status === 'Canceled') {
      return 'Canceled';
    }
    if (trace.isPriceReached === null) {
      return 'Unavailable';
    }

    return trace.isPriceReached ? 'Target reached' : 'Not reached';
  }

  close(): void {
    this.dialogRef.close();
  }
}
