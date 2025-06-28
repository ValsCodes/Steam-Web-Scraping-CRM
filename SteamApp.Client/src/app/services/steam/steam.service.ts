import { Injectable } from '@angular/core';
import { HttpClient, HttpErrorResponse } from '@angular/common/http';
import { Observable, throwError, of } from 'rxjs';
import { catchError, map } from 'rxjs/operators';
import { Product } from '../../models/product.model';

@Injectable({
  providedIn: 'root',
})
export class SteamService {
  constructor(private http: HttpClient) {}

   private readonly localHost: string = "https://localhost:7273/";
  //private readonly localHost: string = 'https://localhost:44347/';

  private readonly get: string = 'get';

  private readonly steam: string = 'steam/';
  private readonly swagger: string = 'swagger/index.html';

  private readonly weaponListings: string = 'weapon-listing-urls';
  private readonly hatListings: string = 'hat-listing-urls';

  checkServerStatus(): Observable<boolean> {
    return this.http
      .get(this.localHost + this.swagger, { responseType: 'text' })
      .pipe(
        map(() => true),
        catchError(() => of(false))
      );
  }

  getWeaponUrls(fromIndex: number, batchSize: number): Observable<string[]> {
    const url = `${this.localHost}${this.steam}weapon/urls/fromPage/${fromIndex}/batchSize/${batchSize}`;
    return this.http.get<string[]>(url).pipe(
      catchError(this.handleError) // Error handling
    );
  }
 
  getHatUrls(fromPage: number, batchSize: number): Observable<string[]> {
    const url = `${this.localHost}${this.steam}hat/urls/fromPage/${fromPage}/batchSize/${batchSize}`;
    return this.http.get<string[]>(url).pipe(
      catchError(this.handleError) // Error handling
    );
  }

  getScrapedPage(page: number): Observable<any> {
    const url = `${this.localHost}${this.steam}hat/page/${page}`;
    return this.http.get<any>(url).pipe(
      catchError(this.handleError) // Error handling
    );
  }

  getBulkPage(page: number): Observable<any> {
    const url = `${this.localHost}${this.steam}hat/page/${page}/bulk`;
    return this.http.get<any>(url).pipe(
      catchError(this.handleError) // Error handling
    );
  }

  getScrapedPagePaintedOnly(page: number): Observable<any> {
    const url = `${this.localHost}${this.steam}hat/page/${page}/painted`;
    console.log('Request URL:', url);
    return this.http.get<any>(url).pipe(
      catchError(this.handleError) // Error handling
    );
  }

  getIsHatPainted(name: string): Observable<any> {
    const url = `${this.localHost}${this.steam}hat/name/${name}/is-painted`;
    return this.http.get<any>(url).pipe(
      catchError(this.handleError) // Error handling
    );
  }


  // Error handling function
  private handleError(error: HttpErrorResponse) {
    // In a real-world app, you may want to send the error to a remote logging infrastructure
    let errorMessage = '';
    if (error.error instanceof ErrorEvent) {
      // A client-side or network error occurred. Handle it accordingly.
      errorMessage = `An error occurred: ${error.error.message}`;
    } else {
      // The backend returned an unsuccessful response code.
      // The response body may contain clues as to what went wrong.
      errorMessage = `Server returned code: ${error.status}, error message is: ${error.message}`;
    }
    console.error(errorMessage);
    return throwError(
      () => new Error('Something went wrong; please try again later.')
    );
  }
}
