import { HttpClient } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { catchError } from 'rxjs/operators';

import {
  ProductTag,
  CreateProductTag
} from '../../models/index';

import { handleError } from '../error-handler';
import * as g from '../general-data';

@Injectable({
  providedIn: 'root',
})
export class ProductTagService {
  private readonly controller = 'api/product-tags';
  private readonly baseUrl = `${g.localHost}${this.controller}`;

  constructor(private http: HttpClient) {}

  getAll(): Observable<ProductTag[]> {
    return this.http
      .get<ProductTag[]>(this.baseUrl)
      .pipe(catchError(handleError));
  }

  exists(productId: number, tagId: number): Observable<void> {
    return this.http
      .get<void>(`${this.baseUrl}/${productId}/${tagId}`)
      .pipe(catchError(handleError));
  }

  getByProduct(productId: number): Observable<ProductTag[]> {
    return this.http
      .get<ProductTag[]>(`${this.baseUrl}/product/${productId}`)
      .pipe(catchError(handleError));
  }

  create(input: CreateProductTag): Observable<void> {
    return this.http
      .post<void>(this.baseUrl, input)
      .pipe(catchError(handleError));
  }

  delete(productId: number, tagId: number): Observable<void> {
    return this.http
      .delete<void>(`${this.baseUrl}/${productId}/${tagId}`)
      .pipe(catchError(handleError));
  }
}