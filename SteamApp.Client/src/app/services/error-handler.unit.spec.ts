import { HttpErrorResponse } from '@angular/common/http';

import { ErrorDialogBridge } from './error-dialog-bridge';
import { handleError } from './error-handler';

describe('handleError unit tests', () => {
  let consoleErrorSpy: jasmine.Spy;
  let openDialogSpy: jasmine.Spy;

  beforeEach(() => {
    consoleErrorSpy = spyOn(console, 'error');
    openDialogSpy = spyOn(ErrorDialogBridge, 'open');
  });

  it('opens the error dialog with API messages', (done) => {
    const error = new HttpErrorResponse({
      status: 400,
      error: { message: 'Game URL is required' },
    });

    handleError(error).subscribe({
      error: (actualError) => {
        expect(actualError).toBe(error);
        expect(consoleErrorSpy).toHaveBeenCalledWith('Game URL is required');
        expect(openDialogSpy).toHaveBeenCalledWith('Game URL is required');
        done();
      },
    });
  });

  it('does not show a dialog for authentication failures', (done) => {
    const error = new HttpErrorResponse({ status: 401, statusText: 'Unauthorized' });

    handleError(error).subscribe({
      error: (actualError) => {
        expect(actualError).toBe(error);
        expect(consoleErrorSpy).not.toHaveBeenCalled();
        expect(openDialogSpy).not.toHaveBeenCalled();
        done();
      },
    });
  });

  it('falls back to the Error message for non-HTTP errors', (done) => {
    const error = new Error('Network unavailable');

    handleError(error).subscribe({
      error: (actualError) => {
        expect(actualError).toBe(error);
        expect(openDialogSpy).toHaveBeenCalledWith('Network unavailable');
        done();
      },
    });
  });
});
