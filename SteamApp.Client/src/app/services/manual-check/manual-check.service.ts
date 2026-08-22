import { HttpClient, HttpParams } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { catchError } from 'rxjs/operators';

import {
  ManualCheckConditionOperator,
  ManualCheckPreset,
  ManualCheckPresetWrite,
  ManualCheckRunAccepted,
  ManualCheckRunDetail,
  ManualCheckRunRequest,
  ManualCheckRunSummary,
} from '../../models';
import { handleError } from '../error-handler';
import * as g from '../general-data';

@Injectable({ providedIn: 'root' })
export class ManualCheckService {
  private readonly baseUrl = `${g.localHost}api/manual-checks`;

  constructor(private readonly http: HttpClient) {}

  getConditionOperators(): Observable<ManualCheckConditionOperator[]> {
    return this.http
      .get<ManualCheckConditionOperator[]>(`${this.baseUrl}/condition-operators`)
      .pipe(catchError(handleError));
  }

  getPresets(gameId?: number): Observable<ManualCheckPreset[]> {
    const params = gameId === undefined
      ? undefined
      : new HttpParams().set('gameId', gameId);
    return this.http
      .get<ManualCheckPreset[]>(`${this.baseUrl}/presets`, { params })
      .pipe(catchError(handleError));
  }

  createPreset(input: ManualCheckPresetWrite): Observable<ManualCheckPreset> {
    return this.http
      .post<ManualCheckPreset>(`${this.baseUrl}/presets`, input)
      .pipe(catchError(handleError));
  }

  updatePreset(id: number, input: ManualCheckPresetWrite): Observable<ManualCheckPreset> {
    return this.http
      .put<ManualCheckPreset>(`${this.baseUrl}/presets/${id}`, input)
      .pipe(catchError(handleError));
  }

  deletePreset(id: number): Observable<void> {
    return this.http
      .delete<void>(`${this.baseUrl}/presets/${id}`)
      .pipe(catchError(handleError));
  }

  createRun(input: ManualCheckRunRequest): Observable<ManualCheckRunAccepted> {
    return this.http
      .post<ManualCheckRunAccepted>(`${this.baseUrl}/runs`, input)
      .pipe(catchError(handleError));
  }

  getRuns(gameId?: number, take = 100): Observable<ManualCheckRunSummary[]> {
    let params = new HttpParams().set('take', take);
    if (gameId !== undefined) {
      params = params.set('gameId', gameId);
    }
    return this.http
      .get<ManualCheckRunSummary[]>(`${this.baseUrl}/runs`, { params })
      .pipe(catchError(handleError));
  }

  getRun(id: number): Observable<ManualCheckRunDetail> {
    return this.http
      .get<ManualCheckRunDetail>(`${this.baseUrl}/runs/${id}`)
      .pipe(catchError(handleError));
  }

  cancelRun(id: number): Observable<ManualCheckRunDetail> {
    return this.http
      .post<ManualCheckRunDetail>(`${this.baseUrl}/runs/${id}/cancel`, {})
      .pipe(catchError(handleError));
  }

  pauseRun(id: number): Observable<ManualCheckRunDetail> {
    return this.http
      .post<ManualCheckRunDetail>(`${this.baseUrl}/runs/${id}/pause`, {})
      .pipe(catchError(handleError));
  }

  continueRun(id: number): Observable<ManualCheckRunAccepted> {
    return this.http
      .post<ManualCheckRunAccepted>(`${this.baseUrl}/runs/${id}/continue`, {})
      .pipe(catchError(handleError));
  }

  rerun(id: number): Observable<ManualCheckRunAccepted> {
    return this.http
      .post<ManualCheckRunAccepted>(`${this.baseUrl}/runs/${id}/rerun`, {})
      .pipe(catchError(handleError));
  }
}
