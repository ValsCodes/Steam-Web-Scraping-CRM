import { Injectable } from '@angular/core';
import { HttpClient, HttpErrorResponse } from '@angular/common/http';
import { Observable, throwError, of } from 'rxjs';
import { catchError, map } from 'rxjs/operators';
import { Product, CreateProduct } from '../../models/product.model';

@Injectable({
  providedIn: 'root',
})
export class ProductService {
  constructor(private http: HttpClient) {}

  private readonly localHost: string = 'https://localhost:44347/';

  private readonly productController: string = 'api/products';
  private readonly swagger: string = 'swagger/index.html';

  checkServerStatus(): Observable<boolean> {
    return this.http
      .get(this.localHost + this.swagger, { responseType: 'text' })
      .pipe(
        map(() => true),
        catchError(() => of(false))
      );
  }

  getProducts(): Observable<Product[]> {
    const url = `${this.localHost}${this.productController}`;
    return this.http.get<Product[]>(url).pipe(
      catchError(this.handleError)
    );
  }

  createProducts(products: CreateProduct[]): Observable<any[]> {
    const url = `${this.localHost}${this.productController}/batch`;

    return this.http.post<any[]>(url, products).pipe(
      catchError(this.handleError) 
    );
  }

  updateProducts(products: Product[]): Observable<any[]> {
    const url = `${this.localHost}${this.productController}/batch`;

    return this.http.put<any[]>(url, products).pipe(
      catchError(this.handleError) 
    );
  }

  deleteProducts(productIds: number[]): Observable<any[]> {
    const url = `${this.localHost}${this.productController}/batch`;
 
    return this.http.delete<any[]>(url, { body: productIds }).pipe(
      catchError(this.handleError)
    );
  }


  private handleError(error: HttpErrorResponse) {
    let errorMessage = '';
    if (error.error instanceof ErrorEvent) {
      errorMessage = `An error occurred: ${error.error.message}`;
    } else {
      errorMessage = `Server returned code: ${error.status}, error message is: ${error.message}`;
    }
    console.error(errorMessage);
    return throwError(
      () => new Error('Something went wrong; please try again later.')
    );
  }
}
