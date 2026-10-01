import { ChangeDetectionStrategy, Component, inject } from '@angular/core';
import { MatButtonModule } from '@angular/material/button';
import { MAT_DIALOG_DATA, MatDialogModule, MatDialogRef } from '@angular/material/dialog';

import {
  ManualCheckCriterion,
  ManualCheckProductResult,
  ManualCheckProductTrace,
} from '../../models';
import { ManualCheckMatchTreeComponent } from './manual-check-match-tree.component';

export interface ManualCheckMatchesDialogData {
  match: ManualCheckProductResult;
  criteria: ManualCheckCriterion[];
  trace: ManualCheckProductTrace | null;
}

@Component({
  selector: 'steam-manual-check-matches-dialog',
  standalone: true,
  changeDetection: ChangeDetectionStrategy.OnPush,
  imports: [MatButtonModule, MatDialogModule, ManualCheckMatchTreeComponent],
  template: `
    <h2 mat-dialog-title>{{ data.match.productName }}</h2>
    <mat-dialog-content class="matches-dialog">
      <div class="matches-dialog__summary">
        <strong>Matched · {{ data.match.matchedAssets.length }} asset{{ data.match.matchedAssets.length === 1 ? '' : 's' }}</strong>
        <span>Cheapest checked: {{ formatCheapestPrice() }}</span>
      </div>

      <steam-manual-check-match-tree
        [assets]="data.match.matchedAssets"
        [criteria]="data.criteria"
        expandedByDefault="all" />
    </mat-dialog-content>
    <mat-dialog-actions align="end">
      <button mat-flat-button type="button" (click)="dialogRef.close()">Close</button>
    </mat-dialog-actions>
  `,
  styles: [`
    .matches-dialog { min-width: min(42rem, 88vw); }
    .matches-dialog__summary { display: flex; flex-wrap: wrap; align-items: center; justify-content: space-between; gap: .75rem; margin-bottom: 1rem; border-radius: .5rem; background: #f1f5f9; padding: .75rem; color: #334155; }
    .matches-dialog__summary strong { color: #166534; }
    @media (max-width: 600px) { .matches-dialog { min-width: 0; } }
  `],
})
export class ManualCheckMatchesDialogComponent {
  readonly data = inject<ManualCheckMatchesDialogData>(MAT_DIALOG_DATA);
  readonly dialogRef = inject<MatDialogRef<ManualCheckMatchesDialogComponent>>(MatDialogRef);

  formatCheapestPrice(): string {
    const price = this.data.trace?.lowestCheckedPriceMinorUnits;
    if (price == null) {
      return 'Price unavailable';
    }

    return new Intl.NumberFormat('en-US', {
      style: 'currency',
      currency: this.data.trace?.priceCurrencyCode || 'EUR',
      minimumFractionDigits: 2,
      maximumFractionDigits: 2,
    }).format(price / 100);
  }
}
