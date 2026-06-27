import { TestBed } from '@angular/core/testing';
import { Router } from '@angular/router';

import {
  EXTERNAL_LINK_DISCLOSURE_STORAGE_KEY,
  ExternalLinkDisclosureService,
} from './external-link-disclosure.service';

describe('ExternalLinkDisclosureService', () => {
  let service: ExternalLinkDisclosureService;
  let router: { url: string; navigateByUrl: jasmine.Spy };

  beforeEach(() => {
    localStorage.removeItem(EXTERNAL_LINK_DISCLOSURE_STORAGE_KEY);

    router = {
      url: '/manual-mode-v2',
      navigateByUrl: jasmine.createSpy('navigateByUrl').and.resolveTo(true),
    };

    TestBed.configureTestingModule({
      providers: [
        ExternalLinkDisclosureService,
        { provide: Router, useValue: router },
      ],
    });

    service = TestBed.inject(ExternalLinkDisclosureService);
  });

  afterEach(() => {
    localStorage.removeItem(EXTERNAL_LINK_DISCLOSURE_STORAGE_KEY);
  });

  it('starts unaccepted and persists acceptance in local storage', () => {
    expect(service.hasAccepted()).toBeFalse();

    service.accept();

    expect(service.hasAccepted()).toBeTrue();
    expect(localStorage.getItem(EXTERNAL_LINK_DISCLOSURE_STORAGE_KEY)).toBe('accepted');
  });

  it('builds a disclosure route with a safe target and internal return URL', () => {
    const disclosureUrl = service.getDisclosureUrl(
      'https://backpack.tf/stats',
      '/manual-mode-v2?tab=items',
    );
    const params = new URLSearchParams(disclosureUrl.split('?')[1]);

    expect(disclosureUrl.startsWith('/external-link-disclosure?')).toBeTrue();
    expect(params.get('target')).toBe('https://backpack.tf/stats');
    expect(params.get('returnTo')).toBe('/manual-mode-v2?tab=items');
  });

  it('routes untrusted web URLs to disclosure before acceptance', () => {
    const result = service.openTrustedUrl('https://evil.example/market', '/manual-mode-v2');
    const disclosureUrl = router.navigateByUrl.calls.mostRecent().args[0] as string;
    const params = new URLSearchParams(disclosureUrl.split('?')[1]);

    expect(result).toBe('needs-disclosure');
    expect(params.get('target')).toBe('https://evil.example/market');
    expect(params.get('returnTo')).toBe('/manual-mode-v2');
  });

  it('routes custom scheme URLs to disclosure before acceptance', () => {
    const result = service.openTrustedUrl('javascript:alert(1)', '/manual-mode-v2');
    const disclosureUrl = router.navigateByUrl.calls.mostRecent().args[0] as string;
    const params = new URLSearchParams(disclosureUrl.split('?')[1]);

    expect(result).toBe('needs-disclosure');
    expect(params.get('target')).toBe('javascript:alert(1)');
    expect(params.get('returnTo')).toBe('/manual-mode-v2');
  });

  it('blocks empty targets without navigating or opening a window', () => {
    const openSpy = spyOn(window, 'open').and.returnValue(null);

    const result = service.openTrustedUrl('   ', '/manual-mode-v2');

    expect(result).toBe('blocked');
    expect(openSpy).not.toHaveBeenCalled();
    expect(router.navigateByUrl).not.toHaveBeenCalled();
  });

  it('routes to disclosure before the user has accepted', () => {
    const result = service.openTrustedUrl('https://steamcommunity.com/market/', '/manual-mode-v2');
    const disclosureUrl = router.navigateByUrl.calls.mostRecent().args[0] as string;
    const params = new URLSearchParams(disclosureUrl.split('?')[1]);

    expect(result).toBe('needs-disclosure');
    expect(disclosureUrl.startsWith('/external-link-disclosure?')).toBeTrue();
    expect(params.get('target')).toBe('https://steamcommunity.com/market/');
    expect(params.get('returnTo')).toBe('/manual-mode-v2');
  });

  it('opens trusted URLs after the user has accepted', () => {
    const openSpy = spyOn(window, 'open').and.returnValue(null);
    service.accept();

    const result = service.openTrustedUrl('https://backpack.tf/stats', '/manual-mode-v2');

    expect(result).toBe('opened');
    expect(openSpy).toHaveBeenCalledWith(
      'https://backpack.tf/stats',
      '_blank',
      'noopener,noreferrer',
    );
    expect(router.navigateByUrl).not.toHaveBeenCalled();
  });

  it('opens untrusted web URLs after the user has accepted', () => {
    const openSpy = spyOn(window, 'open').and.returnValue(null);
    service.accept();

    const result = service.openTrustedUrl('http://evil.example/market', '/manual-mode-v2');

    expect(result).toBe('opened');
    expect(openSpy).toHaveBeenCalledWith(
      'http://evil.example/market',
      '_blank',
      'noopener,noreferrer',
    );
  });

  it('opens custom scheme URLs after the user has accepted', () => {
    const openSpy = spyOn(window, 'open').and.returnValue(null);
    service.accept();

    const result = service.openTrustedUrl('steam://openurl/https://steamcommunity.com/market/', '/manual-mode-v2');

    expect(result).toBe('opened');
    expect(openSpy).toHaveBeenCalledWith(
      'steam://openurl/https://steamcommunity.com/market/',
      '_blank',
      'noopener,noreferrer',
    );
  });
});
