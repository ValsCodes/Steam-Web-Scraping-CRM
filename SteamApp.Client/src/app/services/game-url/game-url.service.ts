import { HttpClient } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { catchError } from 'rxjs/operators';

import {
  GameUrl,
  CreateGameUrl,
  UpdateGameUrl
} from '../../models/game-url.model';

import { handleError } from '../error-handler';
import * as g from '../general-data';

@Injectable({
  providedIn: 'root'
})
export class GameUrlService {
  private readonly controller = 'api/game-urls';
  private readonly baseUrl = `${g.localHost}${this.controller}`;

  constructor(private http: HttpClient) {}

  getAll(): Observable<GameUrl[]> {
    return this.http
      .get<GameUrl[]>(this.baseUrl)
      .pipe(catchError(handleError));
  }

  getById(id: number): Observable<GameUrl> {
    return this.http
      .get<GameUrl>(`${this.baseUrl}/${id}`)
      .pipe(catchError(handleError));
  }

  create(input: CreateGameUrl): Observable<GameUrl> {
    return this.http
      .post<GameUrl>(this.baseUrl, input)
      .pipe(catchError(handleError));
  }

  update(id: number, input: UpdateGameUrl): Observable<void> {
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
