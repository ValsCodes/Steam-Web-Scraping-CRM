import { TestBed } from '@angular/core/testing';
import { Subject } from 'rxjs';

import { LoadingStateService } from './loading-state.service';

describe('LoadingStateService unit tests', () => {
  let service: LoadingStateService;

  beforeEach(() => {
    TestBed.configureTestingModule({});
    service = TestBed.inject(LoadingStateService);
  });

  afterEach(() => {
    document.documentElement.classList.remove('app-loading');
    document.body.classList.remove('app-loading');
  });

  it('tracks overlapping operations without dropping the loading state early', () => {
    expect(service.isLoading()).toBeFalse();

    service.begin();
    service.begin();

    expect(service.isLoading()).toBeTrue();

    service.end();
    expect(service.isLoading()).toBeTrue();

    service.end();
    expect(service.isLoading()).toBeFalse();
  });

  it('never lets pending operations go below zero', () => {
    service.end();

    expect(service.isLoading()).toBeFalse();
  });

  it('wraps observable subscriptions with begin and end', () => {
    const source = new Subject<string>();
    const received: string[] = [];

    service.track(source).subscribe((value) => received.push(value));

    expect(service.isLoading()).toBeTrue();

    source.next('loaded');
    source.complete();

    expect(received).toEqual(['loaded']);
    expect(service.isLoading()).toBeFalse();
  });
});
