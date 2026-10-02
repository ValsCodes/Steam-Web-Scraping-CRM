import { provideHttpClient } from '@angular/common/http';
import { HttpTestingController, provideHttpClientTesting } from '@angular/common/http/testing';
import { TestBed } from '@angular/core/testing';

import { WishListService } from './wish-list.service';

describe('WishListService', () => {
  let service: WishListService;
  let http: HttpTestingController;

  beforeEach(() => {
    TestBed.configureTestingModule({
      providers: [provideHttpClient(), provideHttpClientTesting()],
    });
    service = TestBed.inject(WishListService);
    http = TestBed.inject(HttpTestingController);
  });

  afterEach(() => http.verify());

  it('starts a fresh price check with POST', () => {
    service.check(7).subscribe();

    const request = http.expectOne((candidate) =>
      candidate.url.endsWith('/api/wish-list/7/checks'));
    expect(request.request.method).toBe('POST');
    expect(request.request.body).toEqual({});
    request.flush(createTrace());
  });

  it('loads the requested price-check history page', () => {
    service.getCheckHistory(7, 2, 50).subscribe((page) => {
      expect(page.pageNumber).toBe(2);
    });

    const request = http.expectOne((candidate) =>
      candidate.url.endsWith('/api/wish-list/7/checks'));
    expect(request.request.method).toBe('GET');
    expect(request.request.params.get('pageNumber')).toBe('2');
    expect(request.request.params.get('pageSize')).toBe('50');
    request.flush({ items: [createTrace()], pageNumber: 2, pageSize: 50, totalCount: 60, totalPages: 2 });
  });
});

function createTrace() {
  return {
    id: 1,
    wishListId: 7,
    gameName: 'Portal',
    source: 'Manual',
    status: 'Succeeded',
    targetPrice: 5,
    currentPrice: 4,
    isPriceReached: true,
    requestedAtUtc: '2026-10-02T12:00:00Z',
    startedAtUtc: '2026-10-02T12:00:00Z',
    completedAtUtc: '2026-10-02T12:00:01Z',
    durationMilliseconds: 1000,
    correlationId: 'trace-1',
    errorCode: null,
    errorText: null,
  };
}
