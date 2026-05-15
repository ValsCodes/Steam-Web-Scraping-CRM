import {
  HttpErrorResponse,
  HttpEvent,
  HttpHandler,
  HttpRequest,
  HttpResponse,
} from '@angular/common/http';
import { Router } from '@angular/router';
import { firstValueFrom, Observable, of, throwError } from 'rxjs';

import { AuthInterceptor } from './auth.interceptor';
import { AuthService } from './auth.service';

describe('AuthInterceptor unit tests', () => {
  let auth: jasmine.SpyObj<AuthService>;
  let router: jasmine.SpyObj<Router>;
  let interceptor: AuthInterceptor;

  beforeEach(() => {
    auth = jasmine.createSpyObj<AuthService>('AuthService', [
      'expireSession',
      'getToken',
      'isLoggedIn',
    ]);
    router = jasmine.createSpyObj<Router>('Router', ['navigate'], { url: '/games' });
    router.navigate.and.returnValue(Promise.resolve(true));

    interceptor = new AuthInterceptor(auth, router);
  });

  it('adds the bearer token to API requests for an active session', async () => {
    const forwardedRequests: HttpRequest<unknown>[] = [];
    auth.getToken.and.returnValue('active-token');
    auth.isLoggedIn.and.returnValue(true);

    const handler = handleWith((request) => {
      forwardedRequests.push(request);
      return of(new HttpResponse({ status: 200 }));
    });

    await firstValueFrom(
      interceptor.intercept(
        new HttpRequest('GET', 'https://localhost:7273/api/games'),
        handler,
      ),
    );

    expect(forwardedRequests[0].headers.get('Authorization')).toBe('Bearer active-token');
  });

  it('skips auth endpoints and non-api requests', async () => {
    auth.getToken.and.returnValue('active-token');
    auth.isLoggedIn.and.returnValue(true);
    const forwardedUrls: string[] = [];

    const handler = handleWith((request) => {
      forwardedUrls.push(request.url);
      return of(new HttpResponse({ status: 200 }));
    });

    await firstValueFrom(
      interceptor.intercept(
        new HttpRequest('POST', 'https://localhost:7273/api/Auth/login', {}),
        handler,
      ),
    );
    await firstValueFrom(
      interceptor.intercept(
        new HttpRequest('GET', 'https://example.test/api/games'),
        handler,
      ),
    );

    expect(auth.isLoggedIn).not.toHaveBeenCalled();
    expect(forwardedUrls).toEqual([
      'https://localhost:7273/api/Auth/login',
      'https://example.test/api/games',
    ]);
  });

  it('adds the bearer token to authenticated auth endpoints', async () => {
    const forwardedRequests: HttpRequest<unknown>[] = [];
    auth.getToken.and.returnValue('active-token');
    auth.isLoggedIn.and.returnValue(true);

    const handler = handleWith((request) => {
      forwardedRequests.push(request);
      return of(new HttpResponse({ status: 200 }));
    });

    await firstValueFrom(
      interceptor.intercept(
        new HttpRequest('GET', 'https://localhost:7273/api/Auth/profile'),
        handler,
      ),
    );

    expect(forwardedRequests[0].headers.get('Authorization')).toBe('Bearer active-token');
  });

  it('expires the session before sending a request when the token is already invalid', async () => {
    auth.getToken.and.returnValue('expired-token');
    auth.isLoggedIn.and.returnValue(false);

    await expectAsync(
      firstValueFrom(
        interceptor.intercept(
          new HttpRequest('GET', 'https://localhost:7273/api/games'),
          handleWith(() => of(new HttpResponse({ status: 200 }))),
        ),
      ),
    ).toBeRejectedWith(jasmine.any(HttpErrorResponse));

    expect(auth.expireSession).toHaveBeenCalled();
    expect(router.navigate).toHaveBeenCalledWith(['/session-expired'], { replaceUrl: true });
  });

  it('expires the session when an authenticated API request returns 401', async () => {
    auth.getToken.and.returnValue('active-token');
    auth.isLoggedIn.and.returnValue(true);

    await expectAsync(
      firstValueFrom(
        interceptor.intercept(
          new HttpRequest('GET', 'https://localhost:7273/api/games'),
          handleWith(() =>
            throwError(() => new HttpErrorResponse({ status: 401, statusText: 'Unauthorized' })),
          ),
        ),
      ),
    ).toBeRejectedWith(jasmine.any(HttpErrorResponse));

    expect(auth.expireSession).toHaveBeenCalled();
    expect(router.navigate).toHaveBeenCalledWith(['/session-expired'], { replaceUrl: true });
  });
});

function handleWith(
  handler: (request: HttpRequest<unknown>) => Observable<HttpEvent<unknown>>,
): HttpHandler {
  return { handle: handler };
}
