import { HttpClient } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { catchError } from 'rxjs/operators';

import { CreateItemGroup, ItemGroup } from '../../models';
import { handleError } from '../error-handler';
import * as g from '../general-data';

@Injectable({
  providedIn: 'root',
})
export class ItemGroupService {
  private readonly controller = 'api/item-groups';
  private readonly baseUrl = `${g.localHost}${this.controller}`;

  constructor(private readonly http: HttpClient) {}

  getByGame(gameId: number): Observable<ItemGroup[]> {
    return this.http
      .get<ItemGroup[]>(`${this.baseUrl}/game/${gameId}`)
      .pipe(catchError(handleError));
  }

  create(input: CreateItemGroup): Observable<ItemGroup> {
    return this.http
      .post<ItemGroup>(this.baseUrl, input)
      .pipe(catchError(handleError));
  }
}
