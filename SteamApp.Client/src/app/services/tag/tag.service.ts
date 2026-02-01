import { HttpClient } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { catchError } from 'rxjs/operators';

import {
  Tag,
  CreateTag,
  UpdateTag
} from '../../models/index';

import { handleError } from '../error-handler';
import * as g from '../general-data';

@Injectable({
  providedIn: 'root'
})
export class TagService {
  private readonly controller = 'api/tags';
  private readonly baseUrl = `${g.localHost}${this.controller}`;

  constructor(private http: HttpClient) {}

  getAll(): Observable<Tag[]> {
    return this.http
      .get<Tag[]>(this.baseUrl)
      .pipe(catchError(handleError));
  }

  getById(id: number): Observable<Tag> {
    return this.http
      .get<Tag>(`${this.baseUrl}/${id}`)
      .pipe(catchError(handleError));
  }

  getByGame(gameId: number): Observable<Tag[]> {
    return this.http
      .get<Tag[]>(`${this.baseUrl}/game/${gameId}`)
      .pipe(catchError(handleError));
  }

  create(input: CreateTag): Observable<Tag> {
    return this.http
      .post<Tag>(this.baseUrl, input)
      .pipe(catchError(handleError));
  }

  update(id: number, input: UpdateTag): Observable<void> {
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