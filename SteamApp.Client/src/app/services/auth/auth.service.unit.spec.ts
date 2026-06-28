import { HttpClient, provideHttpClient } from '@angular/common/http';
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
    service.ngOnDestroy();
    localStorage.clear();
    sessionStorage.clear();
  });

  it('clears invalid persisted tokens when the service is created', () => {
    expect(localStorage.getItem('access_token')).toBeNull();
    expect(sessionStorage.getItem('access_token')).toBeNull();
    expect(service.hasToken()).toBeFalse();
  });

  it('hydrates a valid persisted token for new tabs', () => {
    const token = createJwt({
      sub: 'user-1',
      name: 'Val',
      email: 'val@example.test',
      scope: 'user',
      exp: Math.floor(Date.now() / 1000) + 3600,
    });

    localStorage.setItem('access_token', token);

    const hydratedService = new AuthService(TestBed.inject(HttpClient));

    expect(hydratedService.getToken()).toBe(token);
    expect(hydratedService.isLoggedIn()).toBeTrue();
    expect(hydratedService.getCurrentUser()?.displayName).toBe('Val');

    hydratedService.ngOnDestroy();
  });

  it('syncs login and logout changes from other tabs', () => {
    const token = createJwt({
      sub: 'user-2',
      name: 'Other Tab User',
      exp: Math.floor(Date.now() / 1000) + 3600,
    });

    window.dispatchEvent(new StorageEvent('storage', {
      key: 'access_token',
      newValue: token,
      storageArea: localStorage,
    }));

    expect(service.getToken()).toBe(token);
    expect(service.isLoggedIn()).toBeTrue();
    expect(service.getCurrentUser()?.displayName).toBe('Other Tab User');

    window.dispatchEvent(new StorageEvent('storage', {
      key: 'access_token',
      newValue: null,
      storageArea: localStorage,
    }));

    expect(service.getToken()).toBeNull();
    expect(service.isLoggedIn()).toBeFalse();
    expect(service.getCurrentUser()).toBeNull();
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
    expect(localStorage.getItem('access_token')).toBe(token);
    expect(sessionStorage.getItem('access_token')).toBeNull();
    expect(service.isLoggedIn()).toBeTrue();
    expect(service.getCurrentUser()).toEqual({
      id: 'user-1',
      displayName: 'Val',
      firstName: null,
      lastName: null,
      userName: null,
      email: 'val@example.test',
      phone: null,
      clientId: null,
      scope: 'steam.read',
      roles: [],
      isAdmin: false,
    });
  });

  it('parses single and multiple role claims from the current token', () => {
    const token = createJwt({
      sub: 'admin-1',
      name: 'Admin User',
      role: ['User', 'Admin'],
      'http://schemas.microsoft.com/ws/2008/06/identity/claims/role': 'Auditor',
      exp: Math.floor(Date.now() / 1000) + 3600,
    });

    service.login('admin@example.test', 'Password1').subscribe();
    http.expectOne('https://localhost:7273/api/Auth/login').flush({ token });

    expect(service.getCurrentUser()?.roles).toEqual(['User', 'Admin', 'Auditor']);
    expect(service.getCurrentUser()?.isAdmin).toBeTrue();
  });

  it('stores the registration token and uses username claims as the display name', () => {
    const token = createJwt({
      preferred_username: 'steam-admin',
      exp: Math.floor(Date.now() / 1000) + 3600,
    });

    service.register(
      'admin@example.test',
      'steam-admin',
      'Password1',
      'Steam',
      'Admin',
      '+3595550100',
    ).subscribe();

    const request = http.expectOne('https://localhost:7273/api/Auth/register');
    expect(request.request.method).toBe('POST');
    expect(request.request.body).toEqual({
      firstName: 'Steam',
      lastName: 'Admin',
      email: 'admin@example.test',
      phone: '+3595550100',
      userName: 'steam-admin',
      password: 'Password1',
    });

    request.flush({ token });

    expect(service.isLoggedIn()).toBeTrue();
    expect(service.getCurrentUser()?.displayName).toBe('steam-admin');
  });

  it('loads and publishes the current profile', () => {
    service.getProfile().subscribe(profile => {
      expect(profile.displayName).toBe('Val Tester');
    });

    const request = http.expectOne('https://localhost:7273/api/Auth/profile');
    expect(request.request.method).toBe('GET');

    request.flush({
      id: 'user-1',
      firstName: 'Val',
      lastName: 'Tester',
      userName: 'val',
      email: 'val@example.test',
      phone: '+3595550100',
      displayName: 'Val Tester',
    });

    expect(service.getCurrentUser()).toEqual({
      id: 'user-1',
      displayName: 'Val Tester',
      firstName: 'Val',
      lastName: 'Tester',
      userName: 'val',
      email: 'val@example.test',
      phone: '+3595550100',
      clientId: null,
      scope: 'user',
      roles: [],
      isAdmin: false,
    });
  });

  it('updates profile and password through authenticated profile endpoints', () => {
    service.updateProfile({
      firstName: 'Val',
      lastName: 'Tester',
      userName: 'val',
      email: 'val@example.test',
      phone: '+3595550100',
    }).subscribe();

    const profileRequest = http.expectOne('https://localhost:7273/api/Auth/profile');
    expect(profileRequest.request.method).toBe('PUT');
    expect(profileRequest.request.body).toEqual({
      firstName: 'Val',
      lastName: 'Tester',
      userName: 'val',
      email: 'val@example.test',
      phone: '+3595550100',
    });
    profileRequest.flush({
      id: 'user-1',
      firstName: 'Val',
      lastName: 'Tester',
      userName: 'val',
      email: 'val@example.test',
      phone: '+3595550100',
      displayName: 'Val Tester',
    });

    service.changePassword({
      currentPassword: 'Password1',
      newPassword: 'Password2',
    }).subscribe();

    const passwordRequest = http.expectOne('https://localhost:7273/api/Auth/profile/password');
    expect(passwordRequest.request.method).toBe('PUT');
    expect(passwordRequest.request.body).toEqual({
      currentPassword: 'Password1',
      newPassword: 'Password2',
    });
    passwordRequest.flush(null);
  });

  it('deletes the profile and clears the session', () => {
    const token = createJwt({
      name: 'Val',
      exp: Math.floor(Date.now() / 1000) + 3600,
    });

    service.login('val@example.test', 'Password1').subscribe();
    http.expectOne('https://localhost:7273/api/Auth/login').flush({ token });

    service.deleteProfile({ password: 'Password1' }).subscribe();

    const request = http.expectOne('https://localhost:7273/api/Auth/profile');
    expect(request.request.method).toBe('DELETE');
    expect(request.request.body).toEqual({ password: 'Password1' });

    request.flush(null);

    expect(service.getToken()).toBeNull();
    expect(service.getCurrentUser()).toBeNull();
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
    expect(localStorage.getItem('access_token')).toBeNull();
    expect(service.isLoggedIn()).toBeFalse();
    expect(service.getCurrentUser()).toBeNull();
  });
});
