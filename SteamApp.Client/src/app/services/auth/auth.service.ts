// auth.service.ts
import { Injectable, OnDestroy } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { BehaviorSubject } from 'rxjs';
import { tap } from 'rxjs/operators';
import * as g from '../general-data';

export interface TokenResponse {
  token: string;
  tokenType?: string;
  expiresAtUtc?: string;
}

export interface CurrentUser {
  id: string | null;
  displayName: string;
  firstName: string | null;
  lastName: string | null;
  userName: string | null;
  email: string | null;
  phone: string | null;
  clientId: string | null;
  scope: string | null;
  roles: string[];
  isAdmin: boolean;
}

export interface UserProfile {
  id: string;
  displayName: string;
  firstName: string | null;
  lastName: string | null;
  userName: string | null;
  email: string | null;
  phone: string | null;
}

export interface UpdateUserProfileRequest {
  firstName: string | null;
  lastName: string | null;
  userName: string | null;
  email: string;
  phone: string | null;
}

export interface ChangePasswordRequest {
  currentPassword: string;
  newPassword: string;
}

export interface DeleteUserRequest {
  password: string;
}

@Injectable({ providedIn: 'root' })
export class AuthService implements OnDestroy {
  private readonly tokenKey = 'access_token';
  private accessToken: string | null = this.readPersistedToken();
  private readonly endpoint = `${g.localHost.replace(/\/$/, '')}/api/Auth/`;

  private readonly loggedInSubject =
    new BehaviorSubject<boolean>(this.hasValidToken());
  private readonly currentUserSubject =
    new BehaviorSubject<CurrentUser | null>(this.getCurrentUserFromToken());

  readonly loggedIn$ = this.loggedInSubject.asObservable();
  readonly currentUser$ = this.currentUserSubject.asObservable();

  constructor(private http: HttpClient) {
    if (!this.hasValidToken()) {
      this.clearSessionToken();
    }

    window.addEventListener('storage', this.handleStorageEvent);
  }

  ngOnDestroy(): void {
    window.removeEventListener('storage', this.handleStorageEvent);
  }

  login(emailOrUserName: string, password: string) {
    const url = `${this.endpoint}login`;
    return this.http.post<TokenResponse>(url, { emailOrUserName, password }).pipe(
      tap(res => this.storeSession(res.token))
    );
  }

  register(
    email: string,
    userName: string | null,
    password: string,
    firstName: string | null = null,
    lastName: string | null = null,
    phone: string | null = null,
  ) {
    const url = `${this.endpoint}register`;
    return this.http.post<TokenResponse>(url, {
      firstName,
      lastName,
      email,
      phone,
      userName,
      password,
    }).pipe(
      tap(res => this.storeSession(res.token))
    );
  }

  getProfile() {
    return this.http.get<UserProfile>(`${this.endpoint}profile`).pipe(
      tap(profile => this.publishCurrentUserFromProfile(profile)),
    );
  }

  updateProfile(request: UpdateUserProfileRequest) {
    return this.http.put<UserProfile>(`${this.endpoint}profile`, request).pipe(
      tap(profile => this.publishCurrentUserFromProfile(profile)),
    );
  }

  changePassword(request: ChangePasswordRequest) {
    return this.http.put<void>(`${this.endpoint}profile/password`, request);
  }

  deleteProfile(request: DeleteUserRequest) {
    return this.http.request<void>('DELETE', `${this.endpoint}profile`, { body: request }).pipe(
      tap(() => this.logout()),
    );
  }

  logout(): void {
    this.clearSessionToken();
    this.setSessionState(false);
  }

  expireSession(): void {
    this.clearSessionToken();
    this.setSessionState(false);
  }

  getToken(): string | null {
    return this.accessToken;
  }

  hasToken(): boolean {
    return this.getToken() !== null;
  }

  isLoggedIn(): boolean {
    const loggedIn = this.hasValidToken();
    this.setSessionState(loggedIn);
    return loggedIn;
  }

  getCurrentUser(): CurrentUser | null {
    return this.currentUserSubject.value;
  }

  private hasValidToken(): boolean {
    const payload = this.getTokenPayload();
    if (!payload || typeof payload.exp !== 'number') {
      return false;
    }

    return Date.now() < payload.exp * 1000;
  }

  getTimeBeforeExpiration(): number {
    const payload = this.getTokenPayload();
    if (!payload || typeof payload.exp !== 'number') {
      return 0;
    }

    return payload.exp * 1000 - Date.now();
  }

  private storeSession(token: string): void {
    this.accessToken = token;
    this.persistToken(token);
    this.setSessionState(true);
  }

  private readonly handleStorageEvent = (event: StorageEvent): void => {
    if (event.key !== this.tokenKey && event.key !== null) {
      return;
    }

    this.accessToken = event.newValue;

    if (this.hasValidToken()) {
      this.setSessionState(true);
      return;
    }

    this.accessToken = null;
    this.setSessionState(false);
  };

  private setSessionState(isLoggedIn: boolean): void {
    if (this.loggedInSubject.value !== isLoggedIn) {
      this.loggedInSubject.next(isLoggedIn);
    }

    const currentUser = isLoggedIn ? this.getCurrentUserFromToken() : null;
    this.currentUserSubject.next(currentUser);
  }

  private getTokenPayload(): Record<string, unknown> | null {
    const token = this.getToken();
    if (!token) {
      return null;
    }

    const [, payload] = token.split('.');
    if (!payload) {
      return null;
    }

    try {
      return JSON.parse(this.decodeBase64Url(payload)) as Record<string, unknown>;
    } catch {
      return null;
    }
  }

  private getCurrentUserFromToken(): CurrentUser | null {
    const payload = this.getTokenPayload();

    if (!payload || !this.hasValidToken()) {
      return null;
    }

    const id = this.readStringClaim(
      payload,
      'sub',
      'nameid',
      'http://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameidentifier',
    );
    const email = this.readStringClaim(
      payload,
      'email',
      'http://schemas.xmlsoap.org/ws/2005/05/identity/claims/emailaddress',
    );
    const firstName = this.readStringClaim(
      payload,
      'given_name',
      'http://schemas.xmlsoap.org/ws/2005/05/identity/claims/givenname',
    );
    const lastName = this.readStringClaim(
      payload,
      'family_name',
      'surname',
      'http://schemas.xmlsoap.org/ws/2005/05/identity/claims/surname',
    );
    const userName = this.readStringClaim(
      payload,
      'preferred_username',
      'unique_name',
    );
    const phone = this.readStringClaim(
      payload,
      'phone_number',
      'phone',
      'http://schemas.xmlsoap.org/ws/2005/05/identity/claims/mobilephone',
    );
    const displayName = this.readStringClaim(
      payload,
      'name',
      'preferred_username',
      'http://schemas.xmlsoap.org/ws/2005/05/identity/claims/name',
    );
    const clientId = this.readStringClaim(payload, 'client_id');
    const scope = this.readStringClaim(payload, 'scope');
    const roles = this.readStringArrayClaim(
      payload,
      'role',
      'roles',
      'http://schemas.microsoft.com/ws/2008/06/identity/claims/role',
    );

    return {
      id,
      displayName: this.createDisplayName(firstName, lastName)
        ?? displayName
        ?? userName
        ?? email
        ?? clientId
        ?? id
        ?? 'User',
      firstName,
      lastName,
      userName,
      email,
      phone,
      clientId,
      scope,
      roles,
      isAdmin: this.hasRole(roles, 'Admin'),
    };
  }

  private publishCurrentUserFromProfile(profile: UserProfile): void {
    const currentUser = this.currentUserSubject.value;
    const roles = currentUser?.roles ?? [];

    this.currentUserSubject.next({
      id: profile.id,
      displayName: profile.displayName
        || this.createDisplayName(profile.firstName, profile.lastName)
        || profile.userName
        || profile.email
        || profile.id
        || 'User',
      firstName: profile.firstName,
      lastName: profile.lastName,
      userName: profile.userName,
      email: profile.email,
      phone: profile.phone,
      clientId: null,
      scope: currentUser?.scope ?? 'user',
      roles,
      isAdmin: this.hasRole(roles, 'Admin'),
    });
  }

  private createDisplayName(firstName: string | null, lastName: string | null): string | null {
    const value = [firstName, lastName]
      .map(part => part?.trim())
      .filter((part): part is string => !!part)
      .join(' ');

    return value || null;
  }

  private readStringClaim(
    payload: Record<string, unknown>,
    ...claimNames: string[]
  ): string | null {
    for (const claimName of claimNames) {
      const value = payload[claimName];

      if (typeof value === 'string' && value.trim()) {
        return value;
      }
    }

    return null;
  }

  private readStringArrayClaim(
    payload: Record<string, unknown>,
    ...claimNames: string[]
  ): string[] {
    const values: string[] = [];

    for (const claimName of claimNames) {
      const value = payload[claimName];

      if (typeof value === 'string') {
        values.push(value);
        continue;
      }

      if (Array.isArray(value)) {
        values.push(
          ...value.filter((item): item is string => typeof item === 'string'),
        );
      }
    }

    return values
      .map(value => value.trim())
      .filter((value, index, all) =>
        !!value &&
        all.findIndex(item => item.toLowerCase() === value.toLowerCase()) === index,
      );
  }

  private hasRole(roles: readonly string[], role: string): boolean {
    return roles.some(value => value.toLowerCase() === role.toLowerCase());
  }

  private decodeBase64Url(value: string): string {
    const base64 = value
      .replace(/-/g, '+')
      .replace(/_/g, '/')
      .padEnd(Math.ceil(value.length / 4) * 4, '=');

    return atob(base64);
  }

  private clearSessionToken(): void {
    this.accessToken = null;
    this.clearPersistedToken();
  }

  private persistToken(token: string): void {
    try {
      localStorage.setItem(this.tokenKey, token);
      sessionStorage.removeItem(this.tokenKey);
    } catch {
      // If browser storage is unavailable, keep the current tab session in memory.
    }
  }

  private readPersistedToken(): string | null {
    try {
      return localStorage.getItem(this.tokenKey);
    } catch {
      return null;
    }
  }

  private clearPersistedToken(): void {
    try {
      localStorage.removeItem(this.tokenKey);
      sessionStorage.removeItem(this.tokenKey);
    } catch {
      // Storage may be unavailable in restricted browser contexts.
    }
  }
}
