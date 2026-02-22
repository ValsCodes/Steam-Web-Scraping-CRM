import { TestBed } from '@angular/core/testing';

import { GameUrlProductService } from './game-url-product.service';

describe('GameUrlProductService', () => {
  let service: GameUrlProductService;

  beforeEach(() => {
    TestBed.configureTestingModule({});
    service = TestBed.inject(GameUrlProductService);
  });

  it('should be created', () => {
    expect(service).toBeTruthy();
  });
});
