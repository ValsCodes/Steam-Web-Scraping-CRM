import { ChangeDetectionStrategy, Component, inject } from '@angular/core';
import { MAT_DIALOG_DATA, MatDialogModule } from '@angular/material/dialog';
import { MatButtonModule } from '@angular/material/button';

import { ManualCheckProductTrace } from '../../models';

@Component({
  selector: 'steam-manual-check-steam-result-dialog',
  standalone: true,
  changeDetection: ChangeDetectionStrategy.OnPush,
  imports: [MatDialogModule, MatButtonModule],
  template: `
    <h2 mat-dialog-title>Steam API result</h2>
    <mat-dialog-content class="steam-result">
      <div class="steam-result__summary">
        <strong>{{ data.productName || ('Product #' + data.productId) }}</strong>
        <span>{{ !data.matchEvaluated ? 'Preset evaluation failed' : data.matched ? data.matchedAssetCount + ' matched asset(s)' : 'No preset match' }}</span>
        <code>{{ data.fullUrl }}</code>
      </div>
      <pre tabindex="0" aria-label="Steam API result JSON">{{ formattedResult }}</pre>
    </mat-dialog-content>
    <mat-dialog-actions align="end">
      <button mat-flat-button type="button" mat-dialog-close>Close</button>
    </mat-dialog-actions>
  `,
  styles: [`
    .steam-result { display: flex; min-width: min(58rem, 84vw); flex-direction: column; gap: 1rem; }
    .steam-result__summary { display: flex; flex-direction: column; gap: .3rem; color: #475569; }
    .steam-result__summary strong { color: #0f172a; }
    .steam-result__summary code { overflow-wrap: anywhere; white-space: normal; }
    pre { max-height: 62vh; margin: 0; overflow: auto; border: 1px solid #cbd5e1; border-radius: .5rem; background: #0f172a; padding: 1rem; color: #e2e8f0; font: .8rem/1.5 ui-monospace, SFMono-Regular, Consolas, monospace; white-space: pre; }
    @media (max-width: 800px) { .steam-result { min-width: 0; } }
  `],
})
export class ManualCheckSteamResultDialogComponent {
  readonly data = inject<ManualCheckProductTrace>(MAT_DIALOG_DATA);
  readonly formattedResult = this.formatJson(this.data.steamApiResultJson);

  private formatJson(value: string | null): string {
    if (value === null) {
      return 'No parsed Steam response was recorded for this check.';
    }

    try {
      return JSON.stringify(JSON.parse(value), null, 2);
    } catch {
      return value;
    }
  }
}
