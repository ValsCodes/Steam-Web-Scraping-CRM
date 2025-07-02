import { Injectable } from '@angular/core';
import { HttpClient} from '@angular/common/http';
import { Observable, of } from 'rxjs';
import { catchError, map } from 'rxjs/operators';
import { handleError } from '../error-handler';
import * as g from '../general-data'

@Injectable({
  providedIn: 'root',
})
export class SteamService {
  constructor(private http: HttpClient) {}

  private readonly steam: string = 'steam/';
  private readonly swagger: string = 'swagger/index.html';

  checkServerStatus(): Observable<boolean> {
    return this.http
      .get(g.localHost + this.swagger, { responseType: 'text' })
      .pipe(
        map(() => true),
        catchError(() => of(false))
      );
  }

  getWeaponUrls(fromIndex: number, batchSize: number): Observable<string[]> {
    const url = `${g.localHost}${this.steam}weapon/urls/fromPage/${fromIndex}/batchSize/${batchSize}`;
    return this.http.get<string[]>(url).pipe(
      catchError(handleError)
    );
  }
 
  getHatUrls(fromPage: number, batchSize: number): Observable<string[]> {
    const url = `${g.localHost}${this.steam}hat/urls/fromPage/${fromPage}/batchSize/${batchSize}`;
    return this.http.get<string[]>(url).pipe(
      catchError(handleError)
    );
  }

  getScrapedPage(page: number): Observable<any> {
    const url = `${g.localHost}${this.steam}hat/page/${page}`;
    return this.http.get<any>(url).pipe(
      catchError(handleError)
    );
  }

  getBulkPage(page: number): Observable<any> {
    const url = `${g.localHost}${this.steam}hat/page/${page}/bulk`;
    return this.http.get<any>(url).pipe(
      catchError(handleError)
    );
  }

  getScrapedPagePaintedOnly(page: number): Observable<any> {
    const url = `${g.localHost}${this.steam}hat/page/${page}/painted`;
    console.log('Request URL:', url);
    return this.http.get<any>(url).pipe(
      catchError(handleError)
    );
  }

  getIsHatPainted(name: string): Observable<any> {
    const url = `${g.localHost}${this.steam}hat/name/${name}/is-painted`;
    return this.http.get<any>(url).pipe(
      catchError(handleError)
    );
  }
}
