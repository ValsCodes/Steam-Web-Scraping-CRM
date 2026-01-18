import { TestBed } from '@angular/core/testing';

import { GameUrlService } from './game-url.service';

describe('GameUrlService', () => {
  let service: GameUrlService;

  beforeEach(() => {
    TestBed.configureTestingModule({});
    service = TestBed.inject(GameUrlService);
  });

  it('should be created', () => {
    expect(service).toBeTruthy();
  });
});
