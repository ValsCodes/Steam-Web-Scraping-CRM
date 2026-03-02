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
    this.loggedInSubject.next(false);
  }

  getToken(): string | null {
    return localStorage.getItem(this.tokenKey);
  }

  isLoggedIn(): boolean {
    return this.loggedInSubject.value;
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
