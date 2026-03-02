import { HttpClient } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { catchError } from 'rxjs/operators';

import { handleError } from '../error-handler';
import * as g from '../general-data';
import { CreateGameUrlPixel, GameUrlPixel } from '../../models';

@Injectable({
  providedIn: 'root'
})
export class GameUrlPixelService {
  private readonly controller = 'api/game-url-pixels';
  private readonly baseUrl = `${g.localHost}${this.controller}`;

  constructor(private http: HttpClient) {}

  // GET: /api/game-url-pixels
  getAll(): Observable<GameUrlPixel[]> {
    return this.http
      .get<GameUrlPixel[]>(this.baseUrl)
      .pipe(catchError(handleError));
  }

  // GET: /api/game-url-pixels/{pixelId}/{gameUrlId}
  exists(pixelId: number, gameUrlId: number): Observable<void> {
    return this.http
      .get<void>(`${this.baseUrl}/${pixelId}/${gameUrlId}`)
      .pipe(catchError(handleError));
  }


  // GET: /api/game-url-pixels/{gameUrlId}
  existsByGameUrl(gameUrlId: number): Observable<GameUrlPixel[]> {
    return this.http
      .get<GameUrlPixel[]>(`${this.baseUrl}/${gameUrlId}`)
      .pipe(catchError(handleError));
  }

  // POST: /api/game-url-pixels
  create(input: CreateGameUrlPixel): Observable<void> {
    return this.http
      .post<void>(this.baseUrl, input)
      .pipe(catchError(handleError));
  }

  // DELETE: /api/game-url-pixels/{pixelId}/{gameUrlId}
  delete(pixelId: number, gameUrlId: number): Observable<void> {
    return this.http
      .delete<void>(`${this.baseUrl}/${pixelId}/${gameUrlId}`)
      .pipe(catchError(handleError));
  }
}
