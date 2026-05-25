import { Injectable, inject } from '@angular/core';
import { Router } from '@angular/router';

import { externalUrlWarning, openableExternalUrl } from '../common/url-security';

export type ExternalLinkOpenResult = 'opened' | 'blocked' | 'needs-disclosure';

export const EXTERNAL_LINK_DISCLOSURE_STORAGE_KEY = 'external_link_disclosure_v1';

@Injectable({ providedIn: 'root' })
export class ExternalLinkDisclosureService {
  private readonly router = inject(Router);
  private readonly acceptedValue = 'accepted';
  private readonly fallbackReturnUrl = '/home';

  hasAccepted(): boolean {
    try {
      return localStorage.getItem(EXTERNAL_LINK_DISCLOSURE_STORAGE_KEY) === this.acceptedValue;
    } catch {
      return false;
    }
  }

  accept(): void {
    try {
      localStorage.setItem(EXTERNAL_LINK_DISCLOSURE_STORAGE_KEY, this.acceptedValue);
    } catch {
      // If storage is unavailable, allow the current action but do not persist it.
    }
  }

  getDisclosureUrl(
    targetUrl: string | null | undefined,
    returnUrl: string | null | undefined = this.router.url,
  ): string {
    const params = new URLSearchParams();
    const target = openableExternalUrl(targetUrl) ?? targetUrl?.trim();

    if (target) {
      params.set('target', target);
    }

    params.set('returnTo', this.normalizeReturnUrl(returnUrl));

    return `/external-link-disclosure?${params.toString()}`;
  }

  openTrustedUrl(
    targetUrl: string | null | undefined,
    returnUrl: string | null | undefined = this.router.url,
  ): ExternalLinkOpenResult {
    const openableUrl = openableExternalUrl(targetUrl);

    if (!openableUrl) {
      return 'blocked';
    }

    if (!this.hasAccepted()) {
      void this.router.navigateByUrl(this.getDisclosureUrl(openableUrl, returnUrl));
      return 'needs-disclosure';
    }

    window.open(openableUrl, '_blank', 'noopener,noreferrer');
    return 'opened';
  }

  getWarning(targetUrl: string | null | undefined): string | null {
    return externalUrlWarning(targetUrl);
  }

  normalizeReturnUrl(returnUrl: string | null | undefined): string {
    const rawReturnUrl = returnUrl?.trim();

    if (!rawReturnUrl) {
      return this.fallbackReturnUrl;
    }

    try {
      const url = new URL(rawReturnUrl, window.location.origin);

      if (url.origin !== window.location.origin) {
        return this.fallbackReturnUrl;
      }

      const normalized = `${url.pathname}${url.search}${url.hash}`;

      if (
        !normalized ||
        normalized === '/external-link-disclosure' ||
        normalized.startsWith('/external-link-disclosure?')
      ) {
        return this.fallbackReturnUrl;
      }

      return normalized;
    } catch {
      return this.fallbackReturnUrl;
    }
  }
}
