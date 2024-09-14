import { Injectable } from '@angular/core';
import { HttpClient, HttpErrorResponse } from '@angular/common/http';
import { Observable, throwError } from 'rxjs';
import { catchError } from 'rxjs/operators';

import { IListing } from '../../models/listing.model';

@Injectable({
  providedIn: 'root',
})
export class SteamService {
  constructor(private http: HttpClient) {}

  getListings(): Observable<IListing[]> {
    return this.http.get<IListing[]>('/api/products').pipe(
      catchError(this.handleError) // Error handling
    );
  }

  getPaintedListingsOnly(): Observable<IListing[]> {
    return this.http.get<IListing[]>('/api/painted-products').pipe(
      catchError(this.handleError) // Error handling
    );
  }

  // Example for fetching a boolean result from the API
  isListingPainted(id: string): Observable<boolean> {
    return this.http.get<boolean>(`/api/products/${id}/is-painted`).pipe(
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
    return throwError(() => new Error('Something went wrong; please try again later.'));
  }
}
