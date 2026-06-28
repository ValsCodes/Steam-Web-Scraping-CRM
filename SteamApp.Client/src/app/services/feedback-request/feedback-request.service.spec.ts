import { TestBed } from '@angular/core/testing';
import { provideHttpClient } from '@angular/common/http';
import { HttpTestingController, provideHttpClientTesting } from '@angular/common/http/testing';

import { environment } from '../../../environment/environment';
import {
  FeedbackRequestHistoryAction,
  FeedbackRequestStatus,
  FeedbackRequestType,
} from '../../models/feedback-request.model';
import { FeedbackRequestService } from './feedback-request.service';

describe('FeedbackRequestService', () => {
  let service: FeedbackRequestService;
  let httpMock: HttpTestingController;
  const apiBase = environment.apiBaseUrl.endsWith('/')
    ? environment.apiBaseUrl
    : `${environment.apiBaseUrl}/`;
  const baseUrl = `${apiBase}api/feedback-requests`;

  beforeEach(() => {
    TestBed.configureTestingModule({
      providers: [
        provideHttpClient(),
        provideHttpClientTesting(),
      ],
    });

    service = TestBed.inject(FeedbackRequestService);
    httpMock = TestBed.inject(HttpTestingController);
  });

  afterEach(() => {
    httpMock.verify();
  });

  it('creates feedback requests through the API', () => {
    const payload = {
      type: FeedbackRequestType.Bug,
      title: 'Broken export',
      description: 'The export button ignores filters.',
      area: 'Exports',
    };

    service.create(payload).subscribe((result) => {
      expect(result.title).toBe('Broken export');
      expect(result.status).toBe(FeedbackRequestStatus.Active);
    });

    const request = httpMock.expectOne(baseUrl);
    expect(request.request.method).toBe('POST');
    expect(request.request.body).toEqual(payload);

    request.flush({
      id: 7,
      referenceId: 'FB-000007',
      ...payload,
      status: FeedbackRequestStatus.Active,
      createdAtUtc: '2026-06-27T10:00:00Z',
      updatedAtUtc: '2026-06-27T10:00:00Z',
      statusChangedAtUtc: '2026-06-27T10:00:00Z',
    });
  });

  it('patches request status by id', () => {
    service
      .updateStatus(7, { status: FeedbackRequestStatus.Closed })
      .subscribe((result) => {
        expect(result).toBeNull();
      });

    const request = httpMock.expectOne(`${baseUrl}/7/status`);
    expect(request.request.method).toBe('PATCH');
    expect(request.request.body).toEqual({ status: FeedbackRequestStatus.Closed });

    request.flush(null);
  });

  it('loads request history by ticket id', () => {
    service.getHistory(7).subscribe((result) => {
      expect(result[0].action).toBe(FeedbackRequestHistoryAction.Created);
      expect(result[0].newTitle).toBe('Broken export');
    });

    const request = httpMock.expectOne(`${baseUrl}/7/history`);
    expect(request.request.method).toBe('GET');

    request.flush([
      {
        id: 1,
        feedbackRequestId: 7,
        action: FeedbackRequestHistoryAction.Created,
        createdAtUtc: '2026-06-27T10:00:00Z',
        previousTitle: null,
        newTitle: 'Broken export',
      },
    ]);
  });

  it('loads feedback requests by readable reference id', () => {
    service.getByReference('FB-000007').subscribe((result) => {
      expect(result.id).toBe(7);
      expect(result.referenceId).toBe('FB-000007');
    });

    const request = httpMock.expectOne(`${baseUrl}/reference/FB-000007`);
    expect(request.request.method).toBe('GET');

    request.flush({
      id: 7,
      referenceId: 'FB-000007',
      type: FeedbackRequestType.Bug,
      title: 'Broken export',
      description: 'The export button ignores filters.',
      area: 'Exports',
      status: FeedbackRequestStatus.Active,
      createdAtUtc: '2026-06-27T10:00:00Z',
      updatedAtUtc: '2026-06-27T10:00:00Z',
      statusChangedAtUtc: '2026-06-27T10:00:00Z',
    });
  });
});
