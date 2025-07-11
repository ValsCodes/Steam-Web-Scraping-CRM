import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { tap } from 'rxjs/operators';
import * as g from '../general-data';

export interface TokenResponse { token: string; }

@Injectable({ providedIn: 'root' })
export class AuthService {
  private readonly tokenKey = 'access_token';
  private readonly endpoint = `${g.localHost}api/Auth/token`;

  constructor(private http: HttpClient) {
    // Optionally: on startup you could check expiration here
  }

  login(clientId: string, clientSecret: string) {
    return this.http
      .post<TokenResponse>(this.endpoint, { clientId, clientSecret })
      .pipe(tap(res => {
        localStorage.setItem(this.tokenKey, res.token);
      }));
  }

  logout() {
    localStorage.removeItem(this.tokenKey);
  }

  getToken(): string | null {
    return localStorage.getItem(this.tokenKey);
  }

  isLoggedIn(): boolean {
    const token = this.getToken();
    if (!token) return false;
    const [, payload] = token.split('.');
    if (!payload) return false;
    const exp = JSON.parse(atob(payload)).exp;
    return Date.now() < exp * 1000;
  }
}
