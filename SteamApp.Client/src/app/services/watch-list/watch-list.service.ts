import { HttpClient } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { catchError } from 'rxjs/operators';

import {
  WatchList,
  CreateWatchList,
  UpdateWatchList
} from '../../models/watch-list.model';

import { handleError } from '../error-handler';
import * as g from '../general-data';

@Injectable({
  providedIn: 'root'
})
export class WatchListService {
  private readonly controller = 'api/watch-list';
  private readonly baseUrl = `${g.localHost}${this.controller}`;

  constructor(private readonly http: HttpClient) {}

  getAll(): Observable<WatchList[]> {
    return this.http
      .get<WatchList[]>(this.baseUrl)
      .pipe(catchError(handleError));
  }

  getById(id: number): Observable<WatchList> {
    return this.http
      .get<WatchList>(`${this.baseUrl}/${id}`)
      .pipe(catchError(handleError));
  }

  create(input: CreateWatchList): Observable<WatchList> {
    return this.http
      .post<WatchList>(this.baseUrl, input)
      .pipe(catchError(handleError));
  }

  update(id: number, input: UpdateWatchList): Observable<void> {
    return this.http
      .put<void>(`${this.baseUrl}/${id}`, input)
      .pipe(catchError(handleError));
  }

  delete(id: number): Observable<void> {
    return this.http
      .delete<void>(`${this.baseUrl}/${id}`)
      .pipe(catchError(handleError));
  }
}
