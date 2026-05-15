import { Component, inject } from '@angular/core';
import { MAT_DIALOG_DATA, MatDialogModule, MatDialogRef } from '@angular/material/dialog';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';

export type StatusDialogVariant = 'success' | 'info' | 'warn' | 'error';

export interface StatusDialogData {
  title: string;
  subtitle?: string;
  message: string;
  actionText?: string;
  variant?: StatusDialogVariant;
}

@Component({
  selector: 'app-status-dialog',
  standalone: true,
  imports: [MatDialogModule, MatButtonModule, MatIconModule],
  styleUrl: 'dialog.shared.scss',
  template: `
    <div class="dialog">
      <div class="header">
        <div
          class="icon-wrap"
          [class.success]="variant === 'success'"
          [class.info]="variant === 'info'"
          [class.warn]="variant === 'warn'"
          [class.error]="variant === 'error'">
          <mat-icon>{{ icon }}</mat-icon>
        </div>

        <div>
          <h2 class="title">{{ data.title }}</h2>
          <p class="subtitle">{{ data.subtitle || 'Operation completed.' }}</p>
        </div>
      </div>

      <div class="message-box">
        {{ data.message }}
      </div>

      <div class="actions">
        <button mat-flat-button color="primary" type="button" (click)="close()">
          {{ data.actionText || 'OK' }}
        </button>
      </div>
    </div>
  `,
})
export class StatusDialogComponent {
  private readonly dialogRef = inject(MatDialogRef<StatusDialogComponent>);
  public readonly data = inject<StatusDialogData>(MAT_DIALOG_DATA);

  public get variant(): StatusDialogVariant {
    return this.data.variant ?? 'info';
  }

  public get icon(): string {
    switch (this.variant) {
      case 'success':
        return 'task_alt';
      case 'warn':
        return 'warning_amber';
      case 'error':
        return 'error_outline';
      default:
        return 'info';
    }
  }

  public close(): void {
    this.dialogRef.close();
  }
}
