import { ChangeDetectionStrategy, Component, computed, input } from '@angular/core';

import {
  ManualCheckAssetMatch,
  ManualCheckCriterion,
  ManualCheckDescriptionMatch,
} from '../../models';

interface MatchedCriterionView {
  key: string;
  label: string;
  evidence: string[];
}

@Component({
  selector: 'steam-manual-check-match-tree',
  standalone: true,
  changeDetection: ChangeDetectionStrategy.OnPush,
  template: `
    <div class="match-tree">
      @for (asset of visibleAssets(); track asset.assetId; let matchIndex = $index) {
        <details class="match-tree__match" [open]="isExpandedByDefault(matchIndex)">
          <summary>
            <span class="match-tree__heading">
              <span>Match {{ matchIndex + 1 }}</span>
              <span
                class="match-tree__price"
                [class.match-tree__price--matched]="asset.priceRangeMatched === true"
                [class.match-tree__price--failed]="asset.priceRangeMatched === false">
                {{ formatPrice(asset) }}
              </span>
            </span>
          </summary>
          <div class="match-tree__branch">
            @for (criterion of matchedCriteria(asset); track criterion.key) {
              <div class="match-tree__criterion">
                <strong>{{ criterion.label }}</strong>
                @for (evidence of criterion.evidence; track evidence) {
                  <span>{{ evidence }}</span>
                }
              </div>
            } @empty {
              <span class="match-tree__empty">Matched criteria unavailable for this historical result.</span>
            }
          </div>
        </details>
      }
      @if (hiddenMatchCount() > 0) {
        <span class="match-tree__more">+ {{ hiddenMatchCount() }} more match{{ hiddenMatchCount() === 1 ? '' : 'es' }}</span>
      }
    </div>
  `,
  styles: [`
    .match-tree { display: grid; gap: .4rem; margin-top: .5rem; font-size: .75rem; }
    .match-tree__match { border-left: 2px solid #86efac; padding-left: .55rem; }
    .match-tree__match summary { cursor: pointer; color: #166534; font-weight: 700; }
    .match-tree__heading { display: inline-flex; width: calc(100% - 1rem); align-items: center; justify-content: space-between; gap: .75rem; }
    .match-tree__price { color: #475569; white-space: nowrap; }
    .match-tree__price--matched { color: #15803d; }
    .match-tree__price--failed { color: #b91c1c; }
    .match-tree__branch { display: grid; gap: .35rem; margin: .4rem 0 .2rem .35rem; border-left: 1px solid #cbd5e1; padding-left: .75rem; }
    .match-tree__criterion { position: relative; display: grid; gap: .1rem; color: #334155; }
    .match-tree__criterion::before { position: absolute; top: .55rem; left: -.78rem; width: .55rem; border-top: 1px solid #cbd5e1; content: ''; }
    .match-tree__criterion strong { font-weight: 600; }
    .match-tree__criterion span, .match-tree__empty, .match-tree__more { color: #64748b; }
    .match-tree__more { padding-left: .7rem; }
  `],
})
export class ManualCheckMatchTreeComponent {
  readonly assets = input.required<ManualCheckAssetMatch[]>();
  readonly criteria = input<ManualCheckCriterion[]>([]);
  readonly maxMatches = input<number | null>(null);
  readonly expandedByDefault = input<'none' | 'first' | 'all'>('none');

  readonly visibleAssets = computed(() => {
    const limit = this.maxMatches();
    return limit == null ? this.assets() : this.assets().slice(0, Math.max(0, limit));
  });

  readonly hiddenMatchCount = computed(() => this.assets().length - this.visibleAssets().length);

  isExpandedByDefault(matchIndex: number): boolean {
    const mode = this.expandedByDefault();
    return mode === 'all' || (mode === 'first' && matchIndex === 0);
  }

  formatPrice(asset: ManualCheckAssetMatch): string {
    if (asset.priceMinorUnits == null) {
      return 'Price unavailable';
    }

    return new Intl.NumberFormat('en-US', {
      style: 'currency',
      currency: asset.priceCurrencyCode || 'EUR',
      minimumFractionDigits: 2,
      maximumFractionDigits: 2,
    }).format(asset.priceMinorUnits / 100);
  }

  matchedCriteria(asset: ManualCheckAssetMatch): MatchedCriterionView[] {
    const matches = new Map<number, ManualCheckDescriptionMatch[]>();
    for (const description of asset.descriptions) {
      for (const criterionIndex of description.matchedCriterionIndexes ?? []) {
        if (!Number.isInteger(criterionIndex) || criterionIndex < 0) {
          continue;
        }
        matches.set(criterionIndex, [...(matches.get(criterionIndex) ?? []), description]);
      }
    }

    if (matches.size === 0) {
      return asset.descriptions.map((description, index) => ({
        key: `description-${index}`,
        label: `Matched description ${index + 1}`,
        evidence: [this.descriptionLabel(description)],
      }));
    }

    return [...matches.entries()]
      .sort(([left], [right]) => left - right)
      .map(([criterionIndex, descriptions]) => ({
        key: `criterion-${criterionIndex}`,
        label: this.criterionLabel(criterionIndex),
        evidence: [...new Set(descriptions.map((description) => this.descriptionLabel(description)))],
      }));
  }

  private criterionLabel(index: number): string {
    const criterion = this.criteria()[index];
    if (!criterion) {
      return `Criterion ${index + 1}`;
    }

    const conditions: string[] = [];
    if (criterion.nameContains) {
      conditions.push(`name contains “${criterion.nameContains}”`);
    }
    if (criterion.valueContains) {
      conditions.push(`value contains “${criterion.valueContains}”`);
    }

    return `Criterion ${index + 1}: ${conditions.join(' and ') || 'any description'}`;
  }

  private descriptionLabel(description: ManualCheckDescriptionMatch): string {
    return `${description.name || 'Description'}: ${description.value}`;
  }
}
