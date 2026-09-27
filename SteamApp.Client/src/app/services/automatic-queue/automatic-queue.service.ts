import { HttpClient, HttpParams } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { catchError } from 'rxjs/operators';

import {
  AutomaticQueueDefinition,
  AutomaticQueueRun,
  AutomaticQueueRunAccepted,
  AutomaticQueueWrite,
} from '../../models';
import { handleError } from '../error-handler';
import * as g from '../general-data';

@Injectable({ providedIn: 'root' })
export class AutomaticQueueService {
  private readonly baseUrl = `${g.localHost}api/automatic-check-queues`;

  constructor(private readonly http: HttpClient) {}

  getDefinitions(): Observable<AutomaticQueueDefinition[]> {
    return this.http.get<AutomaticQueueDefinition[]>(this.baseUrl).pipe(catchError(handleError));
  }

  getDefinition(id: number): Observable<AutomaticQueueDefinition> {
    return this.http.get<AutomaticQueueDefinition>(`${this.baseUrl}/${id}`).pipe(catchError(handleError));
  }

  createDefinition(input: AutomaticQueueWrite): Observable<AutomaticQueueDefinition> {
    return this.http.post<AutomaticQueueDefinition>(this.baseUrl, input).pipe(catchError(handleError));
  }

  updateDefinition(id: number, input: AutomaticQueueWrite): Observable<AutomaticQueueDefinition> {
    return this.http.put<AutomaticQueueDefinition>(`${this.baseUrl}/${id}`, input).pipe(catchError(handleError));
  }

  deleteDefinition(id: number): Observable<void> {
    return this.http.delete<void>(`${this.baseUrl}/${id}`).pipe(catchError(handleError));
  }

  startRun(id: number): Observable<AutomaticQueueRunAccepted> {
    return this.http.post<AutomaticQueueRunAccepted>(`${this.baseUrl}/${id}/runs`, {}).pipe(catchError(handleError));
  }

  getRuns(queueId?: number, take = 100): Observable<AutomaticQueueRun[]> {
    let params = new HttpParams().set('take', take);
    if (queueId !== undefined) params = params.set('queueId', queueId);
    return this.http.get<AutomaticQueueRun[]>(`${this.baseUrl}/runs`, { params }).pipe(catchError(handleError));
  }

  getRun(id: number): Observable<AutomaticQueueRun> {
    return this.http.get<AutomaticQueueRun>(`${this.baseUrl}/runs/${id}`).pipe(catchError(handleError));
  }

  pauseRun(id: number): Observable<AutomaticQueueRun> {
    return this.http.post<AutomaticQueueRun>(`${this.baseUrl}/runs/${id}/pause`, {}).pipe(catchError(handleError));
  }

  continueRun(id: number): Observable<AutomaticQueueRun> {
    return this.http.post<AutomaticQueueRun>(`${this.baseUrl}/runs/${id}/continue`, {}).pipe(catchError(handleError));
  }

  cancelRun(id: number): Observable<AutomaticQueueRun> {
    return this.http.post<AutomaticQueueRun>(`${this.baseUrl}/runs/${id}/cancel`, {}).pipe(catchError(handleError));
  }
}
