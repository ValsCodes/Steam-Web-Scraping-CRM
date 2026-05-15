// confirm-dialog.component.ts
import { Component, inject } from '@angular/core';
import { MAT_DIALOG_DATA, MatDialogModule, MatDialogRef } from '@angular/material/dialog';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';

export interface ConfirmDialogData {
  title: string;
  subtitle?: string;
  message: string;
  confirmText?: string;
  cancelText?: string;
}

@Component({
  selector: 'app-confirm-dialog',
  standalone: true,
  imports: [MatDialogModule, MatButtonModule, MatIconModule],
  styleUrl: 'dialog.shared.scss',
  template: `
    <div class="dialog">
      <div class="header">
        <div class="icon-wrap warn">
          <mat-icon>warning_amber</mat-icon>
        </div>

        <div>
          <h2 class="title">{{ data.title }}</h2>
          <p class="subtitle">{{ data.subtitle || 'Please confirm this action.' }}</p>
        </div>
      </div>

      <div class="message-box">
        {{ data.message }}
      </div>

      <div class="actions">
        <button mat-stroked-button type="button" (click)="close(false)">
          {{ data.cancelText || 'Cancel' }}
        </button>

        <button mat-flat-button color="warn" type="button" (click)="close(true)">
          {{ data.confirmText || 'Delete' }}
        </button>
      </div>
    </div>
  `,
})
export class ConfirmDialogComponent {
  private readonly dialogRef = inject(MatDialogRef<ConfirmDialogComponent>);
  public readonly data = inject<ConfirmDialogData>(MAT_DIALOG_DATA);

  public close(result: boolean): void {
    this.dialogRef.close(result);
  }
}