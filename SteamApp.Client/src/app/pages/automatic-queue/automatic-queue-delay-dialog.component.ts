import { DatePipe } from '@angular/common';
import { Component, inject } from '@angular/core';
import { MAT_DIALOG_DATA, MatDialogModule } from '@angular/material/dialog';
import { MatButtonModule } from '@angular/material/button';

import { AutomaticQueueRunBlock } from '../../models';

@Component({
  selector: 'steam-automatic-queue-delay-dialog',
  standalone: true,
  imports: [DatePipe, MatDialogModule, MatButtonModule],
  template: `
    <h2 mat-dialog-title>Timeout details</h2>
    <mat-dialog-content>
      <dl>
        <div><dt>Status</dt><dd>{{ data.status }}</dd></div>
        <div><dt>Configured duration</dt><dd>{{ data.configuration.delaySeconds }} seconds</dd></div>
        <div><dt>Started</dt><dd>{{ data.startedAtUtc ? (data.startedAtUtc | date:'medium') : 'Not started' }}</dd></div>
        <div><dt>Due</dt><dd>{{ data.waitUntilUtc ? (data.waitUntilUtc | date:'medium') : '—' }}</dd></div>
        <div><dt>Completed</dt><dd>{{ data.completedAtUtc ? (data.completedAtUtc | date:'medium') : '—' }}</dd></div>
      </dl>
    </mat-dialog-content>
    <mat-dialog-actions align="end"><button mat-flat-button mat-dialog-close>Close</button></mat-dialog-actions>
  `,
  styles: [`dl { display: grid; gap: .6rem; } dl div { display: grid; grid-template-columns: 10rem 1fr; gap: 1rem; } dt { color: #64748b; } dd { margin: 0; }`],
})
export class AutomaticQueueDelayDialogComponent {
  readonly data = inject<AutomaticQueueRunBlock>(MAT_DIALOG_DATA);
}
