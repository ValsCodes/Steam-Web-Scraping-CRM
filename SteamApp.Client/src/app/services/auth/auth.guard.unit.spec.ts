import { ActivatedRouteSnapshot, Router, UrlTree } from '@angular/router';

import { AuthGuard } from './auth.guard';
import { AuthService } from './auth.service';

describe('AuthGuard unit tests', () => {
  let auth: jasmine.SpyObj<AuthService>;
  let router: jasmine.SpyObj<Router>;
  let guard: AuthGuard;
  let loginTree: UrlTree;
  let sessionExpiredTree: UrlTree;
  let homeTree: UrlTree;

  beforeEach(() => {
    loginTree = { toString: () => '/login' } as UrlTree;
    sessionExpiredTree = { toString: () => '/session-expired' } as UrlTree;
    homeTree = { toString: () => '/home' } as UrlTree;

    auth = jasmine.createSpyObj<AuthService>('AuthService', [
      'isLoggedIn',
      'hasToken',
      'getCurrentUser',
    ]);
    router = jasmine.createSpyObj<Router>('Router', ['createUrlTree']);
    router.createUrlTree.and.callFake((commands: unknown[]) => {
      if (commands[0] === '/session-expired') {
        return sessionExpiredTree;
      }

      if (commands[0] === '/home') {
        return homeTree;
      }

      return loginTree;
    });

    guard = new AuthGuard(auth, router);
  });

  it('allows activation when the current token is valid', () => {
    auth.isLoggedIn.and.returnValue(true);

    expect(guard.canActivate(route())).toBeTrue();
    expect(router.createUrlTree).not.toHaveBeenCalled();
  });

  it('redirects anonymous users to login', () => {
    auth.isLoggedIn.and.returnValue(false);
    auth.hasToken.and.returnValue(false);

    expect(guard.canActivate(route())).toBe(loginTree);
    expect(router.createUrlTree).toHaveBeenCalledWith(['/login']);
  });

  it('redirects users with an invalid token to session expired', () => {
    auth.isLoggedIn.and.returnValue(false);
    auth.hasToken.and.returnValue(true);

    expect(guard.canActivateChild(route())).toBe(sessionExpiredTree);
    expect(router.createUrlTree).toHaveBeenCalledWith(['/session-expired']);
  });

  it('allows admin-only routes for admin users', () => {
    auth.isLoggedIn.and.returnValue(true);
    auth.getCurrentUser.and.returnValue({
      roles: ['User', 'Admin'],
    } as never);

    expect(guard.canActivate(route(['Admin']))).toBeTrue();
  });

  it('blocks admin-only routes for non-admin users', () => {
    auth.isLoggedIn.and.returnValue(true);
    auth.getCurrentUser.and.returnValue({
      roles: ['User'],
    } as never);

    expect(guard.canActivate(route(['Admin']))).toBe(homeTree);
    expect(router.createUrlTree).toHaveBeenCalledWith(['/home']);
  });

  function route(roles?: unknown): ActivatedRouteSnapshot {
    return {
      data: roles === undefined ? {} : { roles },
    } as ActivatedRouteSnapshot;
  }
});
