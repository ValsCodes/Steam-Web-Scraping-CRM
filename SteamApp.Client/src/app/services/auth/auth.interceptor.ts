import { Injectable } from '@angular/core';
import {
  HttpInterceptor, HttpRequest,
  HttpHandler,
  HttpEvent,
  HttpErrorResponse,
} from '@angular/common/http';
import { Observable, catchError, throwError } from 'rxjs';
import { AuthService } from './auth.service';
import { Router } from '@angular/router';
import * as g from '../general-data';

@Injectable()
export class AuthInterceptor implements HttpInterceptor {
  constructor(
    private auth: AuthService,
    private router: Router,
  ) {}

  intercept(
    req: HttpRequest<unknown>,
    next: HttpHandler
  ): Observable<HttpEvent<unknown>> {
    if (this.isAuthRequest(req.url) || !this.isApiRequest(req.url)) {
      return next.handle(req);
    }

    const token = this.auth.getToken();

    if (token && !this.auth.isLoggedIn()) {
      this.handleExpiredSession();

      return throwError(() => new HttpErrorResponse({
        status: 401,
        statusText: 'Session Expired',
        url: req.url,
        error: {
          message: 'Your session has expired. Please sign in again.',
        },
      }));
    }

    const authReq = token
      ? req.clone({ setHeaders: { Authorization: `Bearer ${token}` } })
      : req;

    return next.handle(authReq).pipe(
      catchError((error: unknown) => {
        if (error instanceof HttpErrorResponse && error.status === 401 && token) {
          this.handleExpiredSession();
        }

        return throwError(() => error);
      }),
    );
  }

  private isAuthRequest(url: string): boolean {
    const requestUrl = this.toUrl(url);
    return requestUrl?.pathname.toLowerCase().startsWith('/api/auth/') ?? false;
  }

  private isApiRequest(url: string): boolean {
    const requestUrl = this.toUrl(url);
    const apiBaseUrl = this.toUrl(g.localHost);

    if (!requestUrl || !apiBaseUrl || requestUrl.origin !== apiBaseUrl.origin) {
      return false;
    }

    const path = requestUrl.pathname.toLowerCase();
    return path.startsWith('/api/') || path.startsWith('/steam/');
  }

  private toUrl(url: string): URL | null {
    try {
      return new URL(url, window.location.origin);
    } catch {
      return null;
    }
  }

  private handleExpiredSession(): void {
    this.auth.expireSession();

    if (this.router.url !== '/session-expired') {
      void this.router.navigate(['/session-expired'], { replaceUrl: true });
    }
  }
}
