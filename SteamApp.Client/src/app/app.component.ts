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
import { WatchListPanelComponent } from './components/watch-list-panel/watch-list-panel.component';
import { ErrorDialogService } from './services/error-dialog.service';
import { ErrorDialogBridge } from './services/error-dialog-bridge';
import { LoadingStateService } from './services/loading/loading-state.service';
import { SeoMetaService } from './services/seo/seo-meta.service';
import { AuthService } from './services/auth/auth.service';
import { AutomationService } from './services/automation/automation.service';

@Component({
  selector: 'app-root',
  standalone: true,
  imports: [
    RouterOutlet,
    FormsModule,
    SiteHeaderComponent,
    MatPaginatorModule,
    SiteFooter,
    WatchListPanelComponent,
],
  templateUrl: './app.component.html',
  styleUrl: './app.component.scss',
})
export class AppComponent {
  private readonly destroyRef = inject(DestroyRef);

  title = 'SteamApp';

  public constructor(
    errorDialogService: ErrorDialogService,
    router: Router,
    loadingState: LoadingStateService,
    seoMeta: SeoMetaService,
    authService: AuthService,
    _automationService: AutomationService,
  ) {
    ErrorDialogBridge.initialize(errorDialogService);

    router.events
      .pipe(takeUntilDestroyed(this.destroyRef))
      .subscribe((event) => {
        if (event instanceof NavigationStart) {
          loadingState.begin();
          return;
        }

        if (event instanceof NavigationEnd) {
          loadingState.end();
          authService.recordActivity();
          seoMeta.applyRouteSeo(router.routerState.snapshot.root, event.urlAfterRedirects);
          return;
        }

        if (event instanceof NavigationCancel || event instanceof NavigationError) {
          loadingState.end();
        }
      });
  }
}
