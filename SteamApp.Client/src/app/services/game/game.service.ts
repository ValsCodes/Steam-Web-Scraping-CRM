import { HttpClient } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { catchError } from 'rxjs/operators';

import { Game, CreateGame, UpdateGame } from '../../models/game.model';
import { handleError } from '../error-handler';
import * as g from '../general-data';

@Injectable({
  providedIn: 'root',
})
export class GameService {
    private readonly controller = 'api/games';
  private readonly baseUrl = `${g.localHost}${this.controller}`;

  constructor(private http: HttpClient) {}

  getAll(): Observable<Game[]> {
    return this.http
      .get<Game[]>(this.baseUrl)
      .pipe(catchError(handleError));
  }

  getById(id: number): Observable<Game> {
    return this.http
      .get<Game>(`${this.baseUrl}/${id}`)
      .pipe(catchError(handleError));
  }

  create(input: CreateGame): Observable<Game> {
    return this.http
      .post<Game>(this.baseUrl, input)
      .pipe(catchError(handleError));
  }

  update(id: number, input: UpdateGame): Observable<void> {
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
