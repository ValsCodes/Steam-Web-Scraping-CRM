import { Component, inject } from '@angular/core';
import { MAT_DIALOG_DATA, MatDialogModule } from '@angular/material/dialog';
import { MatButtonModule } from '@angular/material/button';

import { AutomaticQueueBlock } from '../../models';

@Component({
  selector: 'steam-automatic-queue-configuration-dialog',
  standalone: true,
  imports: [MatDialogModule, MatButtonModule],
  template: `
    <h2 mat-dialog-title>Block configuration</h2>
    <mat-dialog-content>
      <dl>
        <div><dt>Type</dt><dd>{{ data.type === 'ManualCheck' ? 'Manual Check' : 'Timeout' }}</dd></div>
        @if (data.type === 'Delay') {
          <div><dt>Duration</dt><dd>{{ data.delaySeconds }} seconds</dd></div>
        } @else {
          <div><dt>Game</dt><dd>{{ data.gameName || ('Game #' + data.gameId) }}</dd></div>
          <div><dt>Game URL</dt><dd>{{ data.gameUrlName || ('Source #' + data.gameUrlId) }}</dd></div>
          <div><dt>Template</dt><dd>{{ data.presetName || data.privateTemplate?.name }}</dd></div>
          <div><dt>Template type</dt><dd>{{ data.templateMode === 'SavedPreset' ? 'Saved preset' : 'Private template' }}</dd></div>
          <div><dt>Products</dt><dd>{{ data.productIds === null ? 'All active products' : data.productIds.length + ' selected' }}</dd></div>
          <div><dt>Steam cache</dt><dd>{{ data.bypassCache ? 'Refresh data' : 'Reuse recent data' }}</dd></div>
        }
      </dl>
    </mat-dialog-content>
    <mat-dialog-actions align="end"><button mat-flat-button mat-dialog-close>Close</button></mat-dialog-actions>
  `,
  styles: [`dl { display: grid; gap: .6rem; } dl div { display: grid; grid-template-columns: 9rem 1fr; gap: 1rem; } dt { color: #64748b; } dd { margin: 0; overflow-wrap: anywhere; }`],
})
export class AutomaticQueueConfigurationDialogComponent {
  readonly data = inject<AutomaticQueueBlock>(MAT_DIALOG_DATA);
}
