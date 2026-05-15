// error-dialog.component.ts
import { Component, inject } from '@angular/core';
import { MAT_DIALOG_DATA, MatDialogModule, MatDialogRef } from '@angular/material/dialog';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';

export interface ErrorDialogData {
  message: string;
}

@Component({
  selector: 'app-error-dialog',
  standalone: true,
  imports: [MatDialogModule, MatButtonModule, MatIconModule],
  styleUrl: 'dialog.shared.scss',
  template: `
    <div class="dialog">
      <div class="header">
        <div class="icon-wrap error">
          <mat-icon>error_outline</mat-icon>
        </div>

        <div>
          <h2 class="title">Request failed</h2>
          <p class="subtitle">The server returned an error.</p>
        </div>
      </div>

      <div class="message-box">
        {{ data.message }}
      </div>

      <div class="actions">
        <button mat-flat-button color="primary" type="button" (click)="close()">
          OK
        </button>
      </div>
    </div>
  `,
})
export class ErrorDialogComponent {
  private readonly dialogRef = inject(MatDialogRef<ErrorDialogComponent>);
  public readonly data = inject<ErrorDialogData>(MAT_DIALOG_DATA);

  public close(): void {
    this.dialogRef.close();
  }
}