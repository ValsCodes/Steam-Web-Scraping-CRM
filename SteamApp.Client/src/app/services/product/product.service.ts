import { HttpClient } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { catchError } from 'rxjs/operators';

import {
  Product,
  CreateProduct,
  UpdateProduct
} from '../../models/product.model';

import { handleError } from '../error-handler';
import * as g from '../general-data';

@Injectable({
  providedIn: 'root'
})
export class ProductService {
  private readonly controller = 'api/products';
  private readonly baseUrl = `${g.localHost}${this.controller}`;

  constructor(private http: HttpClient) {}

  getAll(): Observable<Product[]> {
    return this.http
      .get<Product[]>(this.baseUrl)
      .pipe(catchError(handleError));
  }

  getById(id: number): Observable<Product> {
    return this.http
      .get<Product>(`${this.baseUrl}/${id}`)
      .pipe(catchError(handleError));
  }

  create(input: CreateProduct): Observable<Product> {
    return this.http
      .post<Product>(this.baseUrl, input)
      .pipe(catchError(handleError));
  }

  update(id: number, input: UpdateProduct): Observable<void> {
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
