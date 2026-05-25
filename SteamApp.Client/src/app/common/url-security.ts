const TRUSTED_EXTERNAL_HOST_SUFFIXES = [
  'backpack.tf',
  'steamcommunity.com',
  'steampowered.com',
  'steamstatic.com',
  'akamaihd.net',
] as const;

export function safeExternalUrl(value: string | null | undefined): string | null {
  const url = parseWebUrl(value);
  if (!url) {
    return null;
  }

  if (url.origin === window.location.origin) {
    return url.href;
  }

  return url.protocol === 'https:' && isTrustedExternalHost(url.hostname) ? url.href : null;
}

export function openableExternalUrl(value: string | null | undefined): string | null {
  const rawUrl = value?.trim();
  return rawUrl || null;
}

export function externalUrlWarning(value: string | null | undefined): string | null {
  if (!openableExternalUrl(value)) {
    return 'This link does not include a destination.';
  }

  if (safeExternalUrl(value)) {
    return null;
  }

  return 'Warning: this destination is not verified by SteamApp. Review it carefully before opening.';
}

export function safeExternalImageUrl(value: string | null | undefined): string | null {
  return safeExternalUrl(value);
}

export function openSafeExternalUrl(value: string | null | undefined): boolean {
  const url = safeExternalUrl(value);
  if (!url) {
    return false;
  }

  window.open(url, '_blank', 'noopener,noreferrer');
  return true;
}

function isTrustedExternalHost(hostname: string): boolean {
  const normalized = hostname.toLowerCase();

  return TRUSTED_EXTERNAL_HOST_SUFFIXES.some(
    (suffix) => normalized === suffix || normalized.endsWith(`.${suffix}`),
  );
}

function parseWebUrl(value: string | null | undefined): URL | null {
  const rawUrl = value?.trim();
  if (!rawUrl) {
    return null;
  }

  try {
    const url = new URL(rawUrl, window.location.origin);
    return url.protocol === 'http:' || url.protocol === 'https:' ? url : null;
  } catch {
    return null;
  }
}
