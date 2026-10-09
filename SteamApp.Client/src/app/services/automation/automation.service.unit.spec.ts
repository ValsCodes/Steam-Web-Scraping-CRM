import { provideHttpClient } from '@angular/common/http';
import { HttpTestingController, provideHttpClientTesting } from '@angular/common/http/testing';
import { TestBed } from '@angular/core/testing';
import { BehaviorSubject } from 'rxjs';

import { AutomationUsage } from '../../models';
import { AuthService } from '../auth/auth.service';
import { AutomationService } from './automation.service';

describe('AutomationService', () => {
  let service: AutomationService;
  let http: HttpTestingController;
  let loggedIn: BehaviorSubject<boolean>;
  let auth: jasmine.SpyObj<AuthService>;
  let fetchSpy: jasmine.Spy;
  let visibilityState: DocumentVisibilityState;

  beforeEach(() => {
    sessionStorage.removeItem('steamapp_automation_tab_id');
    loggedIn = new BehaviorSubject(false);
    auth = jasmine.createSpyObj<AuthService>(
      'AuthService',
      ['isLoggedIn', 'getToken'],
      { loggedIn$: loggedIn.asObservable() },
    );
    auth.isLoggedIn.and.returnValue(true);
    auth.getToken.and.returnValue('test-token');
    fetchSpy = spyOn(window, 'fetch').and.returnValue(Promise.resolve(new Response()));
    visibilityState = 'visible';
    spyOnProperty(document, 'visibilityState', 'get').and.callFake(() => visibilityState);

    TestBed.configureTestingModule({
      providers: [
        provideHttpClient(),
        provideHttpClientTesting(),
        { provide: AuthService, useValue: auth },
      ],
    });
    service = TestBed.inject(AutomationService);
    http = TestBed.inject(HttpTestingController);
  });

  afterEach(() => {
    service.ngOnDestroy();
    http.verify();
    sessionStorage.removeItem('steamapp_automation_tab_id');
  });

  it('heartbeats a visible authenticated tab and refreshes its allowance', () => {
    loggedIn.next(true);

    const heartbeat = http.expectOne((request) =>
      request.method === 'PUT' && request.url.includes('/api/session-presence/'));
    const tabId = heartbeat.request.url.split('/').at(-1)!;
    heartbeat.flush(null);
    const usageRequest = http.expectOne((request) =>
      request.method === 'GET' && request.url.endsWith('/api/automation-usage/me'));
    const usage: AutomationUsage = {
      unlimited: false,
      limitSeconds: 900,
      usedSeconds: 120,
      remainingSeconds: 780,
      usageResetAtUtc: null,
      presenceRequired: true,
      presenceActive: true,
      presenceExpiresAtUtc: '2026-10-07T14:00:30Z',
      nextAllowanceAtUtc: null,
    };
    usageRequest.flush(usage);

    expect(sessionStorage.getItem('steamapp_automation_tab_id')).toBe(tabId);
    expect(service.usage()).toEqual(usage);
  });

  it('releases the current tab on visibility loss and session end', () => {
    loggedIn.next(true);
    const heartbeat = http.expectOne((request) => request.method === 'PUT');
    const heartbeatUrl = heartbeat.request.url;
    heartbeat.flush(null, { status: 500, statusText: 'Server error' });

    visibilityState = 'hidden';
    document.dispatchEvent(new Event('visibilitychange'));
    window.dispatchEvent(new CustomEvent('steamapp:session-ending'));

    expect(fetchSpy).toHaveBeenCalledTimes(2);
    expect(fetchSpy.calls.allArgs().every(([url, init]) =>
      url === heartbeatUrl && init?.method === 'DELETE' && init.keepalive === true)).toBeTrue();
  });
});
