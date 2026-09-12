import { HttpClient } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { catchError } from 'rxjs/operators';


import { handleError } from '../error-handler';
import * as g from '../general-data';
import {
  CreateGameUrlProduct,
  GameUrlProduct,
  GameUrlProductCurrentStock,
  GameUrlProductStockHistoryPage,
} from '../../models';

@Injectable({
  providedIn: 'root'
})
export class GameUrlProductService {
  private readonly controller = 'api/game-url-products';
  private readonly baseUrl = `${g.localHost}${this.controller}`;

  constructor(private http: HttpClient) {}

  // GET: /api/game-url-products
  getAll(): Observable<GameUrlProduct[]> {
    return this.http
      .get<GameUrlProduct[]>(this.baseUrl)
      .pipe(catchError(handleError));
  }

  // GET: /api/game-url-products/{productId}/{gameUrlId}
  exists(productId: number, gameUrlId: number): Observable<void> {
    return this.http
      .get<void>(`${this.baseUrl}/${productId}/${gameUrlId}`)
      .pipe(catchError(handleError));
  }

  // GET: /api/game-url-products/{gameUrlId}
  existsByGameUrl(gameUrlId: number): Observable<GameUrlProduct[]> {
    return this.http
      .get<GameUrlProduct[]>(`${this.baseUrl}/${gameUrlId}`)
      .pipe(catchError(handleError));
  }

  // POST: /api/game-url-products
  create(input: CreateGameUrlProduct): Observable<void> {
    return this.http
      .post<void>(this.baseUrl, input)
      .pipe(catchError(handleError));
  }

  // DELETE: /api/game-url-products/{productId}/{gameUrlId}
  delete(productId: number, gameUrlId: number): Observable<void> {
    return this.http
      .delete<void>(`${this.baseUrl}/${productId}/${gameUrlId}`)
      .pipe(catchError(handleError));
  }

  assignCurrentStock(
    productId: number,
    gameUrlId: number,
    currentStock: number,
  ): Observable<GameUrlProductCurrentStock> {
    return this.http
      .put<GameUrlProductCurrentStock>(
        `${this.baseUrl}/${productId}/${gameUrlId}/current-stock`,
        { currentStock },
      )
      .pipe(catchError(handleError));
  }

  incrementCurrentStock(
    productId: number,
    gameUrlId: number,
  ): Observable<GameUrlProductCurrentStock> {
    return this.http
      .patch<GameUrlProductCurrentStock>(
        `${this.baseUrl}/${productId}/${gameUrlId}/current-stock/increment`,
        {},
      )
      .pipe(catchError(handleError));
  }

  decrementCurrentStock(
    productId: number,
    gameUrlId: number,
  ): Observable<GameUrlProductCurrentStock> {
    return this.http
      .patch<GameUrlProductCurrentStock>(
        `${this.baseUrl}/${productId}/${gameUrlId}/current-stock/decrement`,
        {},
      )
      .pipe(catchError(handleError));
  }

  getCurrentStockHistory(
    productId: number,
    gameUrlId: number,
    page = 1,
    pageSize = 25,
  ): Observable<GameUrlProductStockHistoryPage> {
    return this.http
      .get<GameUrlProductStockHistoryPage>(
        `${this.baseUrl}/${productId}/${gameUrlId}/current-stock/history`,
        { params: { page, pageSize } },
      )
      .pipe(catchError(handleError));
  }
}
