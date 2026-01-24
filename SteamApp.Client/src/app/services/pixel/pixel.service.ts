import { HttpClient } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { catchError } from 'rxjs/operators';

import {
  Pixel,
  PixelListItem,
  CreatePixel,
  UpdatePixel
} from '../../models/pixel.model';

import { handleError } from '../error-handler';
import * as g from '../general-data';

@Injectable({ providedIn: 'root' })
export class PixelService {
  private readonly controller = 'api/pixels';
  private readonly baseUrl = `${g.localHost}${this.controller}`;

  constructor(private http: HttpClient) {}

  getAll(): Observable<PixelListItem[]> {
    return this.http
      .get<PixelListItem[]>(this.baseUrl)
      .pipe(catchError(handleError));
  }

  getById(id: number): Observable<Pixel> {
    return this.http
      .get<Pixel>(`${this.baseUrl}/${id}`)
      .pipe(catchError(handleError));
  }

  create(input: CreatePixel): Observable<Pixel> {
    return this.http
      .post<Pixel>(this.baseUrl, input)
      .pipe(catchError(handleError));
  }

  update(id: number, input: UpdatePixel): Observable<void> {
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
