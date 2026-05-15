import { Router, UrlTree } from '@angular/router';

import { AuthGuard } from './auth.guard';
import { AuthService } from './auth.service';

describe('AuthGuard unit tests', () => {
  let auth: jasmine.SpyObj<AuthService>;
  let router: jasmine.SpyObj<Router>;
  let guard: AuthGuard;
  let loginTree: UrlTree;
  let sessionExpiredTree: UrlTree;

  beforeEach(() => {
    loginTree = { toString: () => '/login' } as UrlTree;
    sessionExpiredTree = { toString: () => '/session-expired' } as UrlTree;

    auth = jasmine.createSpyObj<AuthService>('AuthService', ['isLoggedIn', 'hasToken']);
    router = jasmine.createSpyObj<Router>('Router', ['createUrlTree']);
    router.createUrlTree.and.callFake((commands: unknown[]) => {
      if (commands[0] === '/session-expired') {
        return sessionExpiredTree;
      }

      return loginTree;
    });

    guard = new AuthGuard(auth, router);
  });

  it('allows activation when the current token is valid', () => {
    auth.isLoggedIn.and.returnValue(true);

    expect(guard.canActivate()).toBeTrue();
    expect(router.createUrlTree).not.toHaveBeenCalled();
  });

  it('redirects anonymous users to login', () => {
    auth.isLoggedIn.and.returnValue(false);
    auth.hasToken.and.returnValue(false);

    expect(guard.canActivate()).toBe(loginTree);
    expect(router.createUrlTree).toHaveBeenCalledWith(['/login']);
  });

  it('redirects users with an invalid token to session expired', () => {
    auth.isLoggedIn.and.returnValue(false);
    auth.hasToken.and.returnValue(true);

    expect(guard.canActivateChild()).toBe(sessionExpiredTree);
    expect(router.createUrlTree).toHaveBeenCalledWith(['/session-expired']);
  });
});
