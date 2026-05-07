// auth.service.ts
import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { BehaviorSubject } from 'rxjs';
import { tap } from 'rxjs/operators';
import * as g from '../general-data';

export interface TokenResponse {
  token: string;
}

@Injectable({ providedIn: 'root' })
export class AuthService {
  private readonly tokenKey = 'access_token';
  private readonly endpoint = `${g.localHost}api/Auth/`;

  private readonly loggedInSubject =
    new BehaviorSubject<boolean>(this.hasValidToken());

  readonly loggedIn$ = this.loggedInSubject.asObservable();

  constructor(private http: HttpClient) {}

  login(clientId: string, clientSecret: string) {
    const url = `${this.endpoint}token`;
    return this.http.post<TokenResponse>(url, { clientId, clientSecret }).pipe(
      tap(res => {
        localStorage.setItem(this.tokenKey, res.token);
        this.loggedInSubject.next(true);
      })
    );
  }

  logout(): void {
    localStorage.removeItem(this.tokenKey);
    this.setLoggedInState(false);
  }

  expireSession(): void {
    localStorage.removeItem(this.tokenKey);
    this.setLoggedInState(false);
  }

  getToken(): string | null {
    return localStorage.getItem(this.tokenKey);
  }

  hasToken(): boolean {
    return this.getToken() !== null;
  }

  isLoggedIn(): boolean {
    const loggedIn = this.hasValidToken();
    this.setLoggedInState(loggedIn);
    return loggedIn;
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

  private setLoggedInState(isLoggedIn: boolean): void {
    if (this.loggedInSubject.value !== isLoggedIn) {
      this.loggedInSubject.next(isLoggedIn);
    }
  }

  private getTokenPayload(): { exp?: number } | null {
    const token = localStorage.getItem(this.tokenKey);
    if (!token) {
      return null;
    }

    const [, payload] = token.split('.');
    if (!payload) {
      return null;
    }

    try {
      return JSON.parse(atob(payload));
    } catch {
      return null;
    }
  }
}
