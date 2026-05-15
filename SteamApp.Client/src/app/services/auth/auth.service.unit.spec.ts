import { provideHttpClient } from '@angular/common/http';
import { HttpTestingController, provideHttpClientTesting } from '@angular/common/http/testing';
import { TestBed } from '@angular/core/testing';

import { createJwt } from '../../../testing/jwt';
import { AuthService } from './auth.service';

describe('AuthService unit tests', () => {
  let service: AuthService;
  let http: HttpTestingController;

  beforeEach(() => {
    localStorage.setItem('access_token', 'stale-local-token');
    sessionStorage.setItem('access_token', 'stale-session-token');

    TestBed.configureTestingModule({
      providers: [
        provideHttpClient(),
        provideHttpClientTesting(),
      ],
    });

    service = TestBed.inject(AuthService);
    http = TestBed.inject(HttpTestingController);
  });

  afterEach(() => {
    http.verify();
    localStorage.clear();
    sessionStorage.clear();
  });

  it('clears persisted tokens when the service is created', () => {
    expect(localStorage.getItem('access_token')).toBeNull();
    expect(sessionStorage.getItem('access_token')).toBeNull();
    expect(service.hasToken()).toBeFalse();
  });

  it('stores the login token in memory and exposes the current user claims', () => {
    const token = createJwt({
      sub: 'user-1',
      name: 'Val',
      email: 'val@example.test',
      scope: 'steam.read',
      exp: Math.floor(Date.now() / 1000) + 3600,
    });

    service.login('val@example.test', 'Password1').subscribe();

    const request = http.expectOne('https://localhost:7273/api/Auth/login');
    expect(request.request.method).toBe('POST');
    expect(request.request.body).toEqual({
      emailOrUserName: 'val@example.test',
      password: 'Password1',
    });

    request.flush({ token });

    expect(service.getToken()).toBe(token);
    expect(service.isLoggedIn()).toBeTrue();
    expect(service.getCurrentUser()).toEqual({
      id: 'user-1',
      displayName: 'Val',
      email: 'val@example.test',
      clientId: null,
      scope: 'steam.read',
    });
  });

  it('stores the registration token and uses username claims as the display name', () => {
    const token = createJwt({
      preferred_username: 'steam-admin',
      exp: Math.floor(Date.now() / 1000) + 3600,
    });

    service.register('admin@example.test', 'steam-admin', 'Password1').subscribe();

    const request = http.expectOne('https://localhost:7273/api/Auth/register');
    expect(request.request.method).toBe('POST');
    expect(request.request.body).toEqual({
      email: 'admin@example.test',
      userName: 'steam-admin',
      password: 'Password1',
    });

    request.flush({ token });

    expect(service.isLoggedIn()).toBeTrue();
    expect(service.getCurrentUser()?.displayName).toBe('steam-admin');
  });

  it('treats an expired token as logged out', () => {
    const token = createJwt({
      name: 'Expired User',
      exp: Math.floor(Date.now() / 1000) - 60,
    });

    service.login('expired@example.test', 'Password1').subscribe();
    http.expectOne('https://localhost:7273/api/Auth/login').flush({ token });

    expect(service.hasToken()).toBeTrue();
    expect(service.isLoggedIn()).toBeFalse();
    expect(service.getCurrentUser()).toBeNull();
    expect(service.getTimeBeforeExpiration()).toBeLessThan(0);
  });

  it('logout clears the in-memory session without touching new login requests', () => {
    const token = createJwt({
      name: 'Val',
      exp: Math.floor(Date.now() / 1000) + 3600,
    });

    service.login('val@example.test', 'Password1').subscribe();
    http.expectOne('https://localhost:7273/api/Auth/login').flush({ token });

    service.logout();

    expect(service.getToken()).toBeNull();
    expect(service.isLoggedIn()).toBeFalse();
    expect(service.getCurrentUser()).toBeNull();
  });
});
