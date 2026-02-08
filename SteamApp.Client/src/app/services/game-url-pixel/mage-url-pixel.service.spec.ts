import { TestBed } from '@angular/core/testing';

import { MageUrlPixelService } from './mage-url-pixel.service';

describe('MageUrlPixelService', () => {
  let service: MageUrlPixelService;

  beforeEach(() => {
    TestBed.configureTestingModule({});
    service = TestBed.inject(MageUrlPixelService);
  });

  it('should be created', () => {
    expect(service).toBeTruthy();
  });
});
