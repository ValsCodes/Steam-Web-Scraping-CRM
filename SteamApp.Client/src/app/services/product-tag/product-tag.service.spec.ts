import { TestBed } from '@angular/core/testing';

import { ProductTagService } from './product-tag.service';

describe('ProductTagService', () => {
  let service: ProductTagService;

  beforeEach(() => {
    TestBed.configureTestingModule({});
    service = TestBed.inject(ProductTagService);
  });

  it('should be created', () => {
    expect(service).toBeTruthy();
  });
});
