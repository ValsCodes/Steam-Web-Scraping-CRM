import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { catchError } from 'rxjs/operators';
import { handleError } from '../error-handler';
import * as g from '../general-data';
import {
  ScrapeHistory,
  ScrapeHistoryDetail,
  ScrapeHistoryRerunResponse,
  WhishListResponse,
} from '../../models';

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

  console.log('gGameUrlId ' + gameUrlId)

    const url = `${this.baseUrl}scrape-pixels/gameUrl/${gameUrlId}/page/${page}`;
    return this.http.get<any>(url).pipe(
      catchError(handleError)
    );
  }

  checkWishlistItem(wishlistId: number): Observable<WhishListResponse> {
    const url = `${this.baseUrl}check-wishlist/${wishlistId}`;
    return this.http.get<any>(url).pipe(
      catchError(handleError)
    );
  }

  getScrapeHistory(take = 100): Observable<ScrapeHistory[]> {
    const url = `${this.baseUrl}scrape-history`;
    return this.http.get<ScrapeHistory[]>(url, {
      params: { take }
    }).pipe(
      catchError(handleError)
    );
  }

  getScrapeHistoryDetail(id: number): Observable<ScrapeHistoryDetail> {
    const url = `${this.baseUrl}scrape-history/${id}`;
    return this.http.get<ScrapeHistoryDetail>(url).pipe(
      catchError(handleError)
    );
  }

  rerunScrapeHistory(id: number): Observable<ScrapeHistoryRerunResponse> {
    const url = `${this.baseUrl}scrape-history/${id}/rerun`;
    return this.http.post<ScrapeHistoryRerunResponse>(url, {}).pipe(
      catchError(handleError)
    );
  }
}
