// error-handler.ts
import { HttpErrorResponse } from '@angular/common/http';
import { Observable, throwError } from 'rxjs';
import { ErrorDialogBridge } from './error-dialog-bridge';

export function handleError(error: HttpErrorResponse | Error): Observable<never> {
  const errorMessage = getErrorMessage(error);

  console.error(errorMessage);
  ErrorDialogBridge.open(errorMessage);

  return throwError(() => error);
}

function getErrorMessage(error: HttpErrorResponse | Error): string {
  if (error instanceof HttpErrorResponse) {
    if (error.error instanceof ErrorEvent) {
      return `An error occurred: ${error.error.message}`;
    }

    return (
      error.error?.message ??
      error.error?.error ??
      `Server returned code: ${error.status}, error message is: ${error.message}`
    );
  }

  return error.message || 'Something went wrong.';
}