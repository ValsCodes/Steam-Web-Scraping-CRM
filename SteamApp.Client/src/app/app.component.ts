import { Component, DestroyRef, inject } from '@angular/core';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import {
  NavigationCancel,
  NavigationEnd,
  NavigationError,
  NavigationStart,
  Router,
  RouterOutlet,
} from '@angular/router';
import { FormsModule } from '@angular/forms';
import { SiteHeaderComponent } from './components/site-header/site-header.component';
import { MatPaginatorModule } from '@angular/material/paginator';
import { SiteFooter } from "./components/site-footer/site-footer";
import { ErrorDialogService } from './services/error-dialog.service';
import { ErrorDialogBridge } from './services/error-dialog-bridge';
import { LoadingStateService } from './services/loading/loading-state.service';

@Component({
  selector: 'app-root',
  standalone: true,
  imports: [
    RouterOutlet,
    FormsModule,
    SiteHeaderComponent,
    MatPaginatorModule,
    SiteFooter
],
  templateUrl: './app.component.html',
  styleUrl: './app.component.scss',
})
export class AppComponent {
  private readonly destroyRef = inject(DestroyRef);

  title = 'steam-app-angular-client';

  public constructor(
    errorDialogService: ErrorDialogService,
    router: Router,
    loadingState: LoadingStateService,
  ) {
    ErrorDialogBridge.initialize(errorDialogService);

    router.events
      .pipe(takeUntilDestroyed(this.destroyRef))
      .subscribe((event) => {
        if (event instanceof NavigationStart) {
          loadingState.begin();
          return;
        }

        if (
          event instanceof NavigationEnd ||
          event instanceof NavigationCancel ||
          event instanceof NavigationError
        ) {
          loadingState.end();
        }
      });
  }
}
