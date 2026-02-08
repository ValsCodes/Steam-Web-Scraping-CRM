import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { catchError } from 'rxjs/operators';
import { handleError } from '../error-handler';
import * as g from '../general-data';

@Injectable({
  providedIn: 'root'
})
export class SteamService {
  constructor(private http: HttpClient) {}

  private readonly baseUrl: string = `${g.localHost}steam/`;

  scrapePage(gameUrlId: number, page: number): Observable<any> {
    const url = `${this.baseUrl}scrape-page/gameUrl/${gameUrlId}/page/${page}`;
    console.log(url);
    return this.http.get<any>(url).pipe(
      catchError(handleError)
    );
  }

  scrapeFromPublicApi(gameUrlId: number, page: number): Observable<any> {
    const url = `${this.baseUrl}scrape-public-api/gameUrl/${gameUrlId}/page/${page}`;

    console.log(url);
    return this.http.get<any>(url).pipe(
      catchError(handleError)
    );
  }

  getPixelInfo(gameUrlId: number, srcUrl: string): Observable<any> {
    const url = `${this.baseUrl}pixel-info/gameUrl/${gameUrlId}`;
    return this.http.get<any>(url, {
      params: { srcUrl }
    }).pipe(
      catchError(handleError)
    );
  }

  scrapeForPixels(gameUrlId: number, page: number): Observable<any> {
    const url = `${this.baseUrl}scrape-pixels/gameUrl/${gameUrlId}/page/${page}`;
    return this.http.get<any>(url).pipe(
      catchError(handleError)
    );
  }

  checkWishlistItem(wishlistId: number): Observable<any> {
    const url = `${this.baseUrl}check-wishlist/${wishlistId}`;
    return this.http.get<any>(url).pipe(
      catchError(handleError)
    );
  }
}
