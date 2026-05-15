// error-dialog.service.ts
import { Injectable } from '@angular/core';
import { MatDialog } from '@angular/material/dialog';
import { ErrorDialogComponent } from '../components/error-dialog.component';

@Injectable({
  providedIn: 'root',
})
export class ErrorDialogService {
  public constructor(private readonly dialog: MatDialog) {}

  public open(message: string): void {
    this.dialog.open(ErrorDialogComponent, {
      width: '420px',
      data: {
        message,
      },
    });
  }
}