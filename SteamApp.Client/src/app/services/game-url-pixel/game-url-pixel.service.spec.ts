import { TestBed } from '@angular/core/testing';

import { GameUrlPixelService } from './game-url-pixel.service';

describe('GameUrlPixelService', () => {
  let service: GameUrlPixelService;

  beforeEach(() => {
    TestBed.configureTestingModule({});
    service = TestBed.inject(GameUrlPixelService);
  });

  it('should be created', () => {
    expect(service).toBeTruthy();
  });
});
