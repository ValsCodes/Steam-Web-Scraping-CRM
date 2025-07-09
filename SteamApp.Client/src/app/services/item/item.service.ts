import { HttpClient, HttpParams } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { Observable } from 'rxjs/internal/Observable';
import { handleError } from '../error-handler';
import { catchError } from 'rxjs';
import { CreateItem, Item, UpdateItem } from '../../models/item.model';
import * as g from '../general-data';

@Injectable({
  providedIn: 'root',
})
export class ItemService {
  private readonly itemController: string = 'api/items';
  private readonly baseUrl:string = `${g.localHost}${this.itemController}`;

  constructor(private http: HttpClient) {}

  getItems(
    classFilters: number[] = [],
    slotFilters: number[] = [],
    nameFilter: string = ''
  ): Observable<Item[]> {
    let params = new HttpParams();
    if (nameFilter) {
      params = params.set('name', nameFilter);
    }
    classFilters.forEach((id) => {
      params = params.append('classFilters', id.toString());
    });
    slotFilters.forEach((id) => {
      params = params.append('slotFilters', id.toString());
    });

    return this.http.get<Item[]>(this.baseUrl, { params }).pipe(catchError(handleError));
  }

  getItemById(itemId: number): Observable<Item> {
    const url = `${this.baseUrl}/${itemId}`;
    return this.http.get<Item>(url).pipe(catchError(handleError));
  }

  createItem(item: CreateItem): Observable<any> {
    const url = `${g.localHost}${this.itemController}`;

    return this.http.post<any>(url, item).pipe(catchError(handleError));
  }

  updateItem(item: UpdateItem): Observable<any> {
    const url = `${g.localHost}${this.itemController}`;

    return this.http.patch<any>(url, item).pipe(catchError(handleError));
  }

  deleteItem(itemId: number): Observable<any> {
    const url = `${g.localHost}${this.itemController}/${itemId}`;

    return this.http.delete<any>(url).pipe(catchError(handleError));
  }
}
