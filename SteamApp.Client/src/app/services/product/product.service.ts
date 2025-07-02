import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { catchError } from 'rxjs/operators';
import {
  Product,
  CreateProduct,
  UpdateProduct,
} from '../../models/product.model';
import { handleError } from '../error-handler';
import * as g from '../general-data';

@Injectable({
  providedIn: 'root',
})
export class ProductService {
  constructor(private http: HttpClient) {}

  private readonly productController: string = 'api/products';

  getProducts(): Observable<Product[]> {
    const url = `${g.localHost}${this.productController}`;
    return this.http.get<Product[]>(url).pipe(catchError(handleError));
  }

  getProductById(productId: number): Observable<Product> {
    const url = `${g.localHost}${this.productController}/${productId}`;
    return this.http.get<Product>(url).pipe(catchError(handleError));
  }

  createProduct(product: CreateProduct): Observable<any> {
    const url = `${g.localHost}${this.productController}`;

    return this.http.post<any>(url, product).pipe(catchError(handleError));
  }

  updateProduct(product: UpdateProduct): Observable<any> {
    const url = `${g.localHost}${this.productController}`;

    return this.http.patch<any>(url, product).pipe(catchError(handleError));
  }

  deleteProduct(productId: number): Observable<any> {
    const url = `${g.localHost}${this.productController}/${productId}`;

    return this.http.delete<any>(url).pipe(catchError(handleError));
  }

  // Beta
  createProducts(products: CreateProduct[]): Observable<any[]> {
    const url = `${g.localHost}${this.productController}/batch`;

    return this.http.post<any[]>(url, products).pipe(catchError(handleError));
  }

  // TODO not done
  updateProducts(products: Product[]): Observable<any[]> {
    const url = `${g.localHost}${this.productController}/batch`;

    return this.http.patch<any[]>(url, products).pipe(catchError(handleError));
  }

  deleteProducts(productIds: number[]): Observable<any[]> {
    const url = `${g.localHost}${this.productController}/batch`;

    return this.http
      .delete<any[]>(url, { body: productIds })
      .pipe(catchError(handleError));
  }
}
