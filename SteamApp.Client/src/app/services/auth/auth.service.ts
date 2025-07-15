import { Injectable } from '@angular/core';
import { HttpClient} from '@angular/common/http';
import { catchError, tap } from 'rxjs/operators';
import * as g from '../general-data';
import { handleError } from '../error-handler';

export interface TokenResponse { token: string; }

@Injectable({ providedIn: 'root' })
export class AuthService {
  private readonly tokenKey = 'access_token';
  private readonly endpoint = `${g.localHost}api/Auth/`;

  constructor(private http: HttpClient) {}

  login(clientId: string, clientSecret: string) {
    const url = `${this.endpoint}token`;
    return this.http.post<TokenResponse>(url, { clientId, clientSecret }).pipe(
      tap((res) => {
        localStorage.setItem(this.tokenKey, res.token);
      })
    );
  }

  logout() {
    localStorage.removeItem(this.tokenKey);
  }

  getToken(): string | null {
    return localStorage.getItem(this.tokenKey);
  }

  getTimeBeforeExpiration(): number {
    const token = this.getToken();
    if (!token) return 0;
    const [, payload] = token.split('.');
    if (!payload) return 0;
    const expiration = JSON.parse(atob(payload)).exp;

    return expiration * 1000 - Date.now();
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
