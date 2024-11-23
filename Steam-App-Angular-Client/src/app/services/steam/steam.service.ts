import { Injectable } from '@angular/core';
import { HttpClient, HttpErrorResponse } from '@angular/common/http';
import { Observable, throwError, of } from 'rxjs';
import { catchError, map } from 'rxjs/operators';

import { IListing } from '../../models/listing.model';
import { ISellListing } from '../../models/sell.listing.model';

@Injectable({
  providedIn: 'root',
})
export class SteamService {
  constructor(private http: HttpClient) {}

  // private readonly localHost: string = "https://localhost:7273/";
  private readonly localHost: string = "https://localhost:5136/";
  private readonly steam: string = "steam/";
  private readonly sellListing: string = "sell-listings/";
  private readonly swagger: string = "swagger/index.html";

  checkServerStatus(): Observable<boolean> {
    
    return this.http.get(this.localHost + this.swagger, { responseType: 'text' }).pipe(
      map(() => true),  
      catchError(() => of(false))
    );
  }

  getListings(page:number): Observable<IListing[]> {
    const url = `${this.localHost}${this.steam}results/filtered/page_${page}`;
    return this.http.get<IListing[]>(url).pipe(
      catchError(this.handleError) // Error handling
    )
  }

  getPaintedListingsOnly(page:number): Observable<IListing[]> {
    const url = `${this.localHost}${this.steam}results/painted-only/page_${page}`;
    return this.http.get<IListing[]>(url).pipe(
      catchError(this.handleError) // Error handling
    );
  }

  isPaintedListings(name:string): Observable<boolean> {
    const url = `${this.localHost}${this.steam}result/${name}/is_painted`;
    return this.http.get<boolean>(url).pipe(
      catchError(this.handleError) // Error handling
    );
  }

  getSellListings(): Observable<ISellListing[]> {
    const url = `${this.localHost}${this.sellListing}listings`;
    return this.http.get<ISellListing[]>(url).pipe(
      catchError(this.handleError) // Error handling
    );
  }

  createSellListings(sellListing: ISellListing): Observable<ISellListing> {
    const url = `${this.localHost}${this.sellListing}listings`;
    return this.http.post<ISellListing>(url, sellListing).pipe(
      catchError(this.handleError) // Error handling
    );
  }

  updateSellListing(sellListing: ISellListing): Observable<ISellListing> {
    const url = `${this.localHost}${this.sellListing}listings/${sellListing.id}`;
    return this.http.put<ISellListing>(url, sellListing).pipe(
      catchError(this.handleError) // Error handling
    );
  }

  partialUpdateSellListing(id: number, partialUpdate: Partial<ISellListing>): Observable<ISellListing> {
    const url = `${this.localHost}${this.sellListing}listings/${id}`;
    return this.http.patch<ISellListing>(url, partialUpdate).pipe(
      catchError(this.handleError) // Error handling
    );
  }

  deleteSellListing(id: number): Observable<void> {
    const url = `${this.localHost}${this.sellListing}listings/${id}`;
    return this.http.delete<void>(url).pipe(
      catchError(this.handleError) // Error handling
    );
  }

  updateBulkListings(listings: ISellListing[]){
        throw new Error('Method not implemented.');

    // const url = `${this.localHost}${this.sellListing}listings/${id}`;
    // return this.http.delete<void>(url).pipe(
    //   catchError(this.handleError) // Error handling
    // );
  }

  deleteBulkListings(listings: number[]){
    throw new Error('Method not implemented.');

// const url = `${this.localHost}${this.sellListing}listings/${id}`;
// return this.http.delete<void>(url).pipe(
//   catchError(this.handleError) // Error handling
// );
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
    return throwError(() => new Error('Something went wrong; please try again later.'));
  }
}
