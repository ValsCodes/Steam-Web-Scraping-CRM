// error-dialog-bridge.ts
import { ErrorDialogService } from './error-dialog.service';

export class ErrorDialogBridge {
  private static service?: ErrorDialogService;

  public static initialize(service: ErrorDialogService): void {
    ErrorDialogBridge.service = service;
  }

  public static open(message: string): void {
    if (!ErrorDialogBridge.service) {
      window.alert(message);
      return;
    }

    ErrorDialogBridge.service.open(message);
  }
}