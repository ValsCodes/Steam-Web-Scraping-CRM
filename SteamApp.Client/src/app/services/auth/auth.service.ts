// auth.service.ts
import { Injectable } from '@angular/core';
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
  email: string | null;
  clientId: string | null;
  scope: string | null;
}

@Injectable({ providedIn: 'root' })
export class AuthService {
  private readonly tokenKey = 'access_token';
  private accessToken: string | null = null;
  private readonly endpoint = `${g.localHost.replace(/\/$/, '')}/api/Auth/`;

  private readonly loggedInSubject =
    new BehaviorSubject<boolean>(this.hasValidToken());
  private readonly currentUserSubject =
    new BehaviorSubject<CurrentUser | null>(this.getCurrentUserFromToken());

  readonly loggedIn$ = this.loggedInSubject.asObservable();
  readonly currentUser$ = this.currentUserSubject.asObservable();

  constructor(private http: HttpClient) {
    this.clearPersistedToken();
  }

  login(emailOrUserName: string, password: string) {
    const url = `${this.endpoint}login`;
    return this.http.post<TokenResponse>(url, { emailOrUserName, password }).pipe(
      tap(res => this.storeSession(res.token))
    );
  }

  register(email: string, userName: string | null, password: string) {
    const url = `${this.endpoint}register`;
    return this.http.post<TokenResponse>(url, { email, userName, password }).pipe(
      tap(res => this.storeSession(res.token))
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
    this.clearPersistedToken();
    this.setSessionState(true);
  }

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
    const displayName = this.readStringClaim(
      payload,
      'name',
      'unique_name',
      'preferred_username',
      'http://schemas.xmlsoap.org/ws/2005/05/identity/claims/name',
    );
    const clientId = this.readStringClaim(payload, 'client_id');
    const scope = this.readStringClaim(payload, 'scope');

    return {
      id,
      displayName: displayName ?? email ?? clientId ?? id ?? 'User',
      email,
      clientId,
      scope,
    };
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

  private clearPersistedToken(): void {
    localStorage.removeItem(this.tokenKey);
    sessionStorage.removeItem(this.tokenKey);
  }
}
