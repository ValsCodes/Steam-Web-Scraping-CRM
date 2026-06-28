import { HttpClient } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { catchError } from 'rxjs/operators';

import {
  CreateFeedbackRequest,
  FeedbackRequest,
  FeedbackRequestHistory,
  UpdateFeedbackRequest,
  UpdateFeedbackRequestStatus,
} from '../../models/feedback-request.model';
import { handleError } from '../error-handler';
import * as g from '../general-data';

@Injectable({
  providedIn: 'root',
})
export class FeedbackRequestService {
  private readonly controller = 'api/feedback-requests';
  private readonly baseUrl = `${g.localHost}${this.controller}`;

  constructor(private readonly http: HttpClient) {}

  getAll(): Observable<FeedbackRequest[]> {
    return this.http
      .get<FeedbackRequest[]>(this.baseUrl)
      .pipe(catchError(handleError));
  }

  getById(id: number): Observable<FeedbackRequest> {
    return this.http
      .get<FeedbackRequest>(`${this.baseUrl}/${id}`)
      .pipe(catchError(handleError));
  }

  getByReference(referenceId: string): Observable<FeedbackRequest> {
    return this.http
      .get<FeedbackRequest>(`${this.baseUrl}/reference/${encodeURIComponent(referenceId)}`)
      .pipe(catchError(handleError));
  }

  getHistory(id: number): Observable<FeedbackRequestHistory[]> {
    return this.http
      .get<FeedbackRequestHistory[]>(`${this.baseUrl}/${id}/history`)
      .pipe(catchError(handleError));
  }

  create(input: CreateFeedbackRequest): Observable<FeedbackRequest> {
    return this.http
      .post<FeedbackRequest>(this.baseUrl, input)
      .pipe(catchError(handleError));
  }

  update(id: number, input: UpdateFeedbackRequest): Observable<void> {
    return this.http
      .put<void>(`${this.baseUrl}/${id}`, input)
      .pipe(catchError(handleError));
  }

  updateStatus(id: number, input: UpdateFeedbackRequestStatus): Observable<void> {
    return this.http
      .patch<void>(`${this.baseUrl}/${id}/status`, input)
      .pipe(catchError(handleError));
  }
}
