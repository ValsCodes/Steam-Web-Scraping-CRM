import { provideHttpClient } from '@angular/common/http';
import { HttpTestingController, provideHttpClientTesting } from '@angular/common/http/testing';
import { TestBed } from '@angular/core/testing';

import { GameUrlProductService } from './game-url-product.service';

describe('GameUrlProductService', () => {
  let service: GameUrlProductService;
  let http: HttpTestingController;

  beforeEach(() => {
    TestBed.configureTestingModule({
      providers: [provideHttpClient(), provideHttpClientTesting()],
    });
    service = TestBed.inject(GameUrlProductService);
    http = TestBed.inject(HttpTestingController);
  });

  afterEach(() => http.verify());

  it('assigns current stock by both relation IDs', () => {
    service.assignCurrentStock(4, 7, -12).subscribe((result) => {
      expect(result.currentStock).toBe(-12);
    });

    const request = http.expectOne((candidate) =>
      candidate.url.endsWith('/api/game-url-products/4/7/current-stock'));
    expect(request.request.method).toBe('PUT');
    expect(request.request.body).toEqual({ currentStock: -12 });
    request.flush({ currentStock: -12 });
  });

  it('increments and decrements current stock through dedicated patch endpoints', () => {
    service.incrementCurrentStock(4, 7).subscribe();
    const increment = http.expectOne((candidate) =>
      candidate.url.endsWith('/api/game-url-products/4/7/current-stock/increment'));
    expect(increment.request.method).toBe('PATCH');
    expect(increment.request.body).toEqual({});
    increment.flush({ currentStock: 1 });

    service.decrementCurrentStock(4, 7).subscribe();
    const decrement = http.expectOne((candidate) =>
      candidate.url.endsWith('/api/game-url-products/4/7/current-stock/decrement'));
    expect(decrement.request.method).toBe('PATCH');
    decrement.flush({ currentStock: 0 });
  });

  it('loads a requested stock-history page', () => {
    service.getCurrentStockHistory(4, 7, 2, 25).subscribe((result) => {
      expect(result.pageNumber).toBe(2);
    });

    const request = http.expectOne((candidate) =>
      candidate.url.endsWith('/api/game-url-products/4/7/current-stock/history'));
    expect(request.request.method).toBe('GET');
    expect(request.request.params.get('page')).toBe('2');
    expect(request.request.params.get('pageSize')).toBe('25');
    request.flush({ items: [], pageNumber: 2, pageSize: 25, totalCount: 30, totalPages: 2 });
  });
});
