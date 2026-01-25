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
    const token = localStorage.getItem(this.tokenKey);
    if (!token) { return false; }

    const [, payload] = token.split('.');
    if (!payload) { return false; }

    const exp = JSON.parse(atob(payload)).exp;
    return Date.now() < exp * 1000;
  }

  getTimeBeforeExpiration(): number {
  const token = localStorage.getItem(this.tokenKey);
  if (!token) {
    return 0;
  }

  const [, payload] = token.split('.');
  if (!payload) {
    return 0;
  }

  const exp = JSON.parse(atob(payload)).exp;
  return exp * 1000 - Date.now();
}
}
