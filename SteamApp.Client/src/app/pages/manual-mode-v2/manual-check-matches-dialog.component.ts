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
  productName: string;
  match: ManualCheckProductResult | null;
  criteria: ManualCheckCriterion[];
  trace: ManualCheckProductTrace | null;
}

@Component({
  selector: 'steam-manual-check-matches-dialog',
  standalone: true,
  changeDetection: ChangeDetectionStrategy.OnPush,
  imports: [MatButtonModule, MatDialogModule, ManualCheckMatchTreeComponent],
  template: `
    <h2 mat-dialog-title>{{ data.productName }}</h2>
    <mat-dialog-content class="matches-dialog">
      <div class="matches-dialog__summary">
        @if (data.match) {
          <strong>Matched · {{ data.match.matchedAssets.length }} asset{{ data.match.matchedAssets.length === 1 ? '' : 's' }}</strong>
        } @else {
          <strong>No matching assets</strong>
        }
        <span>Cheapest checked: {{ formatCheapestPrice() }}</span>
      </div>

      @if (data.match) {
      <steam-manual-check-match-tree
        [assets]="data.match.matchedAssets"
        [criteria]="data.criteria"
        expandedByDefault="all" />
      }

      <details class="matches-dialog__advanced">
        <summary>Advanced · all checked listings</summary>
        @if (data.trace?.checkedAssets?.length) {
          <div class="matches-dialog__assets">
            @for (asset of data.trace!.checkedAssets; track asset.assetId + ':' + asset.classId + ':' + asset.instanceId) {
              <article class="matches-dialog__asset">
                <div class="matches-dialog__asset-heading">
                  <div>
                    <strong>{{ asset.marketName || ('Asset ' + asset.assetId) }}</strong>
                    <small>{{ asset.appId }} / {{ asset.contextId }} / {{ asset.assetId }}</small>
                  </div>
                  <div class="matches-dialog__badges">
                    <span [class.matches-dialog__badge--matched]="asset.matched">
                      {{ asset.matched ? 'Matched' : 'No match' }}
                    </span>
                    <span>{{ formatAssetPrice(asset.priceMinorUnits, asset.priceCurrencyCode) }}</span>
                    @if (asset.priceRangeMatched !== null && asset.priceRangeMatched !== undefined) {
                      <span>{{ asset.priceRangeMatched ? 'Price in range' : 'Price outside range' }}</span>
                    }
                  </div>
                </div>
                @if (asset.descriptions.length) {
                  <dl>
                    @for (attribute of asset.descriptions; track $index) {
                      <div>
                        <dt>{{ attribute.name || 'Attribute' }}</dt>
                        <dd>{{ attribute.value }}</dd>
                        @if (attribute.matchedCriterionIndexes.length) {
                          <small>Matched criteria: {{ formatCriterionIndexes(attribute.matchedCriterionIndexes) }}</small>
                        }
                      </div>
                    }
                  </dl>
                } @else {
                  <p>No Steam description attributes were returned for this listing.</p>
                }
              </article>
            }
          </div>
        } @else {
          <p class="matches-dialog__fallback">
            Detailed checked-listing data is unavailable. This run may predate advanced listing inspection.
          </p>
        }
      </details>
    </mat-dialog-content>
    <mat-dialog-actions align="end">
      <button mat-flat-button type="button" (click)="dialogRef.close()">Close</button>
    </mat-dialog-actions>
  `,
  styles: [`
    .matches-dialog { min-width: min(42rem, 88vw); }
    .matches-dialog__summary { display: flex; flex-wrap: wrap; align-items: center; justify-content: space-between; gap: .75rem; margin-bottom: 1rem; border-radius: .5rem; background: #f1f5f9; padding: .75rem; color: #334155; }
    .matches-dialog__summary strong { color: #166534; }
    .matches-dialog__advanced { margin-top: 1rem; border: 1px solid #cbd5e1; border-radius: .5rem; padding: .75rem; }
    .matches-dialog__advanced summary { cursor: pointer; font-weight: 700; color: #334155; }
    .matches-dialog__assets { display: grid; gap: .75rem; margin-top: .75rem; }
    .matches-dialog__asset { border: 1px solid #e2e8f0; border-radius: .5rem; padding: .75rem; }
    .matches-dialog__asset-heading { display: flex; flex-wrap: wrap; justify-content: space-between; gap: .75rem; }
    .matches-dialog__asset-heading > div:first-child { display: flex; flex-direction: column; gap: .2rem; }
    .matches-dialog__asset-heading small { color: #64748b; }
    .matches-dialog__badges { display: flex; flex-wrap: wrap; gap: .35rem; }
    .matches-dialog__badges span { border-radius: 999px; background: #e2e8f0; padding: .2rem .55rem; font-size: .75rem; }
    .matches-dialog__badges .matches-dialog__badge--matched { background: #dcfce7; color: #166534; }
    .matches-dialog__asset dl { display: grid; gap: .5rem; margin: .75rem 0 0; }
    .matches-dialog__asset dl div { border-top: 1px solid #f1f5f9; padding-top: .5rem; }
    .matches-dialog__asset dt { font-size: .75rem; font-weight: 700; color: #475569; }
    .matches-dialog__asset dd { margin: .15rem 0 0; overflow-wrap: anywhere; }
    .matches-dialog__asset dl small { color: #2563eb; }
    .matches-dialog__fallback { margin: .75rem 0 0; color: #64748b; }
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

  formatAssetPrice(priceMinorUnits: number | null | undefined, currencyCode: string | null | undefined): string {
    if (priceMinorUnits == null) return 'Price unavailable';
    return new Intl.NumberFormat('en-US', {
      style: 'currency',
      currency: currencyCode || 'EUR',
      minimumFractionDigits: 2,
      maximumFractionDigits: 2,
    }).format(priceMinorUnits / 100);
  }

  formatCriterionIndexes(indexes: number[]): string {
    return indexes.map((index) => index + 1).join(', ');
  }
}
