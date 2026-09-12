import { CommonModule } from '@angular/common';
import { ChangeDetectionStrategy, ChangeDetectorRef, Component, inject, OnInit } from '@angular/core';
import { FormControl, ReactiveFormsModule, Validators } from '@angular/forms';
import { MAT_DIALOG_DATA, MatDialogModule, MatDialogRef } from '@angular/material/dialog';
import { MatButtonModule } from '@angular/material/button';
import { finalize } from 'rxjs';

import {
  GameUrlProductStockHistory,
  GameUrlProductStockOperation,
} from '../../models';
import { GameUrlProductService } from '../../services';

export interface AdvancedStockDialogData {
  productId: number;
  gameUrlId: number;
  productName: string;
  currentStock: number;
}

@Component({
  selector: 'steam-advanced-stock-dialog',
  standalone: true,
  changeDetection: ChangeDetectionStrategy.OnPush,
  imports: [CommonModule, ReactiveFormsModule, MatDialogModule, MatButtonModule],
  template: `
    <h2 mat-dialog-title>Advanced Stock</h2>
    <mat-dialog-content class="advanced-stock-dialog">
      <p class="advanced-stock-dialog__product">{{ data.productName }}</p>

      <section aria-labelledby="currentStockHeading">
        <h3 id="currentStockHeading">Current stock</h3>
        <div class="advanced-stock-dialog__stepper">
          <button mat-stroked-button type="button" aria-label="Decrease current stock"
            [disabled]="busy" (click)="decrement()">−</button>
          <strong aria-live="polite">{{ currentStock }}</strong>
          <button mat-stroked-button type="button" aria-label="Increase current stock"
            [disabled]="busy" (click)="increment()">+</button>
        </div>
      </section>

      <section class="advanced-stock-dialog__assignment" aria-labelledby="assignStockHeading">
        <h3 id="assignStockHeading">Set exact stock</h3>
        <div>
          <input type="number" [formControl]="stockControl" step="1"
            min="-2147483648" max="2147483647" aria-label="Exact current stock" />
          <button mat-flat-button type="button" [disabled]="busy || !isAssignmentValid"
            (click)="assign()">Set stock</button>
        </div>
        @if (stockControl.touched && !isAssignmentValid) {
          <p class="advanced-stock-dialog__error">Enter a whole number from −2,147,483,648 to 2,147,483,647.</p>
        }
      </section>

      <section class="advanced-stock-dialog__history" aria-labelledby="stockHistoryHeading">
        <div class="advanced-stock-dialog__history-heading">
          <h3 id="stockHistoryHeading">Stock history</h3>
          <span>{{ totalCount }} change{{ totalCount === 1 ? '' : 's' }}</span>
        </div>

        @if (historyLoading) {
          <p role="status">Loading history…</p>
        } @else if (history.length === 0) {
          <p>No stock changes have been recorded.</p>
        } @else {
          <div class="advanced-stock-dialog__history-list">
            @for (entry of history; track entry.id) {
              <article>
                <div>
                  <strong>{{ operationLabel(entry.operation) }}</strong>
                  <time [attr.datetime]="entry.createdAtUtc">{{ entry.createdAtUtc | date:'medium' }}</time>
                </div>
                <span>{{ entry.previousStock }} → {{ entry.newStock }}</span>
              </article>
            }
          </div>
        }

        <div class="advanced-stock-dialog__pagination">
          <button mat-stroked-button type="button" [disabled]="historyLoading || pageNumber <= 1"
            (click)="previousPage()">Previous</button>
          <span>Page {{ pageNumber }} of {{ totalPages || 1 }}</span>
          <button mat-stroked-button type="button"
            [disabled]="historyLoading || totalPages === 0 || pageNumber >= totalPages"
            (click)="nextPage()">Next</button>
        </div>
      </section>
    </mat-dialog-content>
    <mat-dialog-actions align="end">
      <button mat-button type="button" [disabled]="busy" (click)="close()">Close</button>
    </mat-dialog-actions>
  `,
  styles: [`
    .advanced-stock-dialog { display: grid; gap: 1.25rem; min-width: min(34rem, 80vw); }
    .advanced-stock-dialog h3 { margin: 0 0 .5rem; font-size: 1rem; }
    .advanced-stock-dialog__product { margin: 0; color: #475569; font-weight: 600; }
    .advanced-stock-dialog__stepper { display: flex; align-items: center; gap: 1rem; }
    .advanced-stock-dialog__stepper strong { min-width: 6rem; text-align: center; font-size: 1.35rem; }
    .advanced-stock-dialog__assignment > div { display: flex; gap: .75rem; align-items: center; }
    .advanced-stock-dialog__assignment input { width: 12rem; border: 1px solid #cbd5e1; border-radius: .375rem; padding: .65rem; }
    .advanced-stock-dialog__error { margin: .5rem 0 0; color: #b91c1c; font-size: .875rem; }
    .advanced-stock-dialog__history { border-top: 1px solid #e2e8f0; padding-top: 1rem; }
    .advanced-stock-dialog__history-heading { display: flex; align-items: baseline; justify-content: space-between; gap: 1rem; }
    .advanced-stock-dialog__history-heading span { color: #64748b; font-size: .875rem; }
    .advanced-stock-dialog__history-list { display: grid; gap: .5rem; max-height: 18rem; overflow: auto; }
    .advanced-stock-dialog__history-list article { display: flex; justify-content: space-between; gap: 1rem; border: 1px solid #e2e8f0; border-radius: .5rem; padding: .65rem .75rem; }
    .advanced-stock-dialog__history-list article div { display: grid; gap: .15rem; }
    .advanced-stock-dialog__history-list time { color: #64748b; font-size: .8rem; }
    .advanced-stock-dialog__pagination { display: flex; align-items: center; justify-content: space-between; gap: 1rem; margin-top: .75rem; }
  `],
})
export class AdvancedStockDialogComponent implements OnInit {
  readonly data = inject<AdvancedStockDialogData>(MAT_DIALOG_DATA);
  private readonly dialogRef = inject<MatDialogRef<AdvancedStockDialogComponent, number>>(MatDialogRef);
  private readonly gameUrlProductService = inject(GameUrlProductService);
  private readonly cdr = inject(ChangeDetectorRef);
  readonly stockControl = new FormControl<number | null>(this.data.currentStock, [
    Validators.required,
    Validators.min(-2147483648),
    Validators.max(2147483647),
  ]);
  readonly pageSize = 25;
  currentStock = this.data.currentStock;
  history: GameUrlProductStockHistory[] = [];
  pageNumber = 1;
  totalCount = 0;
  totalPages = 0;
  busy = false;
  historyLoading = false;

  ngOnInit(): void {
    this.loadHistory(1);
  }

  get isAssignmentValid(): boolean {
    const value = this.stockControl.value;
    return this.stockControl.valid && value !== null && Number.isInteger(value);
  }

  increment(): void {
    if (this.busy) { return; }
    this.runStockOperation(() => this.gameUrlProductService.incrementCurrentStock(
      this.data.productId,
      this.data.gameUrlId,
    ));
  }

  decrement(): void {
    if (this.busy) { return; }
    this.runStockOperation(() => this.gameUrlProductService.decrementCurrentStock(
      this.data.productId,
      this.data.gameUrlId,
    ));
  }

  assign(): void {
    this.stockControl.markAsTouched();
    if (this.busy || !this.isAssignmentValid) { return; }

    this.runStockOperation(() => this.gameUrlProductService.assignCurrentStock(
      this.data.productId,
      this.data.gameUrlId,
      this.stockControl.value!,
    ));
  }

  previousPage(): void {
    if (this.pageNumber > 1) { this.loadHistory(this.pageNumber - 1); }
  }

  nextPage(): void {
    if (this.pageNumber < this.totalPages) { this.loadHistory(this.pageNumber + 1); }
  }

  operationLabel(operation: GameUrlProductStockOperation): string {
    switch (operation) {
      case GameUrlProductStockOperation.Assigned: return 'Assigned';
      case GameUrlProductStockOperation.Incremented: return 'Incremented';
      case GameUrlProductStockOperation.Decremented: return 'Decremented';
      default: return 'Changed';
    }
  }

  close(): void {
    this.dialogRef.close(this.currentStock);
  }

  private runStockOperation(
    operation: () => ReturnType<GameUrlProductService['incrementCurrentStock']>,
  ): void {
    this.busy = true;
    this.cdr.markForCheck();
    operation().pipe(finalize(() => {
      this.busy = false;
      this.cdr.markForCheck();
    })).subscribe({
      next: (result) => {
        this.currentStock = result.currentStock;
        this.stockControl.setValue(result.currentStock);
        this.loadHistory(1);
        this.cdr.markForCheck();
      },
    });
  }

  private loadHistory(page: number): void {
    if (this.historyLoading) { return; }
    this.historyLoading = true;
    this.cdr.markForCheck();
    this.gameUrlProductService.getCurrentStockHistory(
      this.data.productId,
      this.data.gameUrlId,
      page,
      this.pageSize,
    ).pipe(finalize(() => {
      this.historyLoading = false;
      this.cdr.markForCheck();
    })).subscribe({
      next: (result) => {
        this.history = result.items;
        this.pageNumber = result.pageNumber;
        this.totalCount = result.totalCount;
        this.totalPages = result.totalPages;
        this.cdr.markForCheck();
      },
    });
  }
}
