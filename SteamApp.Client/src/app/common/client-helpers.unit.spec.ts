import {
  externalUrlWarning,
  formatMs,
  makeEnumHelpers,
  openableExternalUrl,
  openSafeExternalUrl,
  safeExternalImageUrl,
  safeExternalUrl,
} from './index';
import { encode } from './url.encoder';

describe('client helper unit tests', () => {
  it('encodes route and query values with URL-safe escaping', () => {
    expect(encode('Team Fortress 2 / Unusual Hat')).toBe('Team%20Fortress%202%20%2F%20Unusual%20Hat');
    expect(encode(null as unknown as string)).toBe('');
    expect(encode(undefined as unknown as string)).toBe('');
  });

  it('formats elapsed milliseconds as hh:mm:ss', () => {
    expect(formatMs(0)).toBe('00:00:00');
    expect(formatMs(3661000)).toBe('01:01:01');
    expect(formatMs(25 * 60 * 60 * 1000)).toBe('25:00:00');
  });

  it('creates enum lookup maps and dropdown collections', () => {
    enum Status {
      Active = 1,
      Paused = 2,
    }

    const helpers = makeEnumHelpers(Status);

    expect(helpers.map).toEqual({ 1: 'Active', 2: 'Paused' });
    expect(helpers.collection).toEqual([
      { id: 1, label: 'Active' },
      { id: 2, label: 'Paused' },
    ]);
  });

  it('allows same-origin URLs and trusted https hosts', () => {
    expect(safeExternalUrl('/games')).toContain('/games');
    expect(safeExternalUrl('https://steamcommunity.com/market/listings')).toBe(
      'https://steamcommunity.com/market/listings',
    );
    expect(safeExternalImageUrl('https://cdn.steamstatic.com/image.png')).toBe(
      'https://cdn.steamstatic.com/image.png',
    );
  });

  it('blocks untrusted hosts and insecure external URLs', () => {
    expect(safeExternalUrl('http://steamcommunity.com/market')).toBeNull();
    expect(safeExternalUrl('https://evil.example/market')).toBeNull();
    expect(safeExternalUrl('javascript:alert(1)')).toBeNull();
    expect(safeExternalUrl('')).toBeNull();
  });

  it('allows any non-empty URL string to remain openable while warning when unverified', () => {
    expect(openableExternalUrl('http://steamcommunity.com/market')).toBe(
      'http://steamcommunity.com/market',
    );
    expect(openableExternalUrl('https://evil.example/market')).toBe(
      'https://evil.example/market',
    );
    expect(openableExternalUrl('javascript:alert(1)')).toBe('javascript:alert(1)');
    expect(openableExternalUrl('steam://openurl/https://steamcommunity.com/market/')).toBe(
      'steam://openurl/https://steamcommunity.com/market/',
    );
    expect(openableExternalUrl('mailto:ops@example.test')).toBe('mailto:ops@example.test');
    expect(openableExternalUrl('not a url but the browser decides')).toBe(
      'not a url but the browser decides',
    );
    expect(openableExternalUrl('  ')).toBeNull();
    expect(externalUrlWarning('https://evil.example/market')).toContain('Warning');
    expect(externalUrlWarning('javascript:alert(1)')).toContain('Warning');
    expect(externalUrlWarning('https://steamcommunity.com/market')).toBeNull();
  });

  it('opens only sanitized external URLs', () => {
    const openSpy = spyOn(window, 'open').and.returnValue(null);

    expect(openSafeExternalUrl('https://backpack.tf/stats')).toBeTrue();
    expect(openSpy).toHaveBeenCalledWith(
      'https://backpack.tf/stats',
      '_blank',
      'noopener,noreferrer',
    );

    expect(openSafeExternalUrl('https://example.test/blocked')).toBeFalse();
    expect(openSpy).toHaveBeenCalledTimes(1);
  });
});
