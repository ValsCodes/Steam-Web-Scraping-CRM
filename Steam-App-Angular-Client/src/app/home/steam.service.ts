import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';

import { IListing } from '../home/listing.model';


@Injectable({
  providedIn: 'root',
})
export class SteamService {
  constructor(private http: HttpClient) {}

  getListings(): Observable<IListing[]> {
    return this.http.get<IListing[]>('/api/products');
    // Add correct Api End Point
  }

  getPaintedListingsOnly(): Observable<IListing[]> {
    return this.http.get<IListing[]>('/api/products');
    // Add correct Api End Point
  }

  // getIsListingPainted(): (boolean, string) {
  //   return this.http.get<IListing[]>('/api/products');
  //   // Add correct Api End Point
  // }
}
