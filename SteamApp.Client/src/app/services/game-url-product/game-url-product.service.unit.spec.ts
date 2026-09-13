import { provideHttpClient } from '@angular/common/http';
import { HttpTestingController, provideHttpClientTesting } from '@angular/common/http/testing';
import { TestBed } from '@angular/core/testing';
import { GameUrlProductService } from './game-url-product.service';

describe('GameUrlProductService bulk sync', () => {
  it('sends a single PUT with the full product selection, including an empty selection', () => {
    TestBed.configureTestingModule({ providers: [provideHttpClient(), provideHttpClientTesting()] });
    const service = TestBed.inject(GameUrlProductService);
    const http = TestBed.inject(HttpTestingController);
    for (const productIds of [[1, 2, 3], []]) {
      service.bulkSync(7, productIds).subscribe();
      const request = http.expectOne(candidate => candidate.url.endsWith('/api/game-url-products/7/bulk'));
      expect(request.request.method).toBe('PUT');
      expect(request.request.body).toEqual({ productIds });
      request.flush(null);
    }
    http.verify();
  });
});
