const TRUSTED_EXTERNAL_HOST_SUFFIXES = [
  'backpack.tf',
  'steamcommunity.com',
  'steampowered.com',
  'steamstatic.com',
  'akamaihd.net',
] as const;

export function safeExternalUrl(value: string | null | undefined): string | null {
  const rawUrl = value?.trim();
  if (!rawUrl) {
    return null;
  }

  try {
    const url = new URL(rawUrl, window.location.origin);

    if (url.origin === window.location.origin) {
      return url.href;
    }

    if (url.protocol !== 'https:') {
      return null;
    }

    return isTrustedExternalHost(url.hostname) ? url.href : null;
  } catch {
    return null;
  }
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
