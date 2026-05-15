import { HttpClient } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { catchError } from 'rxjs/operators';

import {
  CreateScrapingMode,
  ScrapingMode,
  UpdateScrapingMode,
} from '../../models';

import { handleError } from '../error-handler';
import * as g from '../general-data';

@Injectable({
  providedIn: 'root',
})
export class ScrapingModeService {
  private readonly controller = 'api/scraping-modes';
  private readonly baseUrl = `${g.localHost}${this.controller}`;

  constructor(private readonly http: HttpClient) {}

  getAll(): Observable<ScrapingMode[]> {
    return this.http
      .get<ScrapingMode[]>(this.baseUrl)
      .pipe(catchError(handleError));
  }

  getById(id: number): Observable<ScrapingMode> {
    return this.http
      .get<ScrapingMode>(`${this.baseUrl}/${id}`)
      .pipe(catchError(handleError));
  }

  create(input: CreateScrapingMode): Observable<ScrapingMode> {
    return this.http
      .post<ScrapingMode>(this.baseUrl, input)
      .pipe(catchError(handleError));
  }

  update(id: number, input: UpdateScrapingMode): Observable<void> {
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
