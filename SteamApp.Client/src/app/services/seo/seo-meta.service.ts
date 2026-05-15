import { DOCUMENT } from '@angular/common';
import { Injectable, inject } from '@angular/core';
import { ActivatedRouteSnapshot } from '@angular/router';
import { Meta, Title } from '@angular/platform-browser';

import { environment } from '../../../environment/environment';

export interface SeoRouteData {
  title?: string;
  description?: string;
  canonicalPath?: string;
  robots?: string;
  imagePath?: string;
  type?: string;
  structuredData?: Record<string, unknown>;
}

const APP_NAME = 'Steam Web Scraping CRM';
const DEFAULT_DESCRIPTION =
  'Centralized administration for Steam market sources, catalog metadata, monitoring, and scraping workflows.';
const DEFAULT_IMAGE_PATH = '/assets/brand/steam-app-social.svg';
const DEFAULT_ROBOTS = 'index,follow';

@Injectable({ providedIn: 'root' })
export class SeoMetaService {
  private readonly document = inject(DOCUMENT);
  private readonly meta = inject(Meta);
  private readonly title = inject(Title);

  applyRouteSeo(route: ActivatedRouteSnapshot, currentUrl: string): void {
    const seo = this.collectSeoData(route);
    const pageTitle = seo.title ?? APP_NAME;
    const title = pageTitle === APP_NAME ? APP_NAME : `${pageTitle} | ${APP_NAME}`;
    const description = seo.description ?? DEFAULT_DESCRIPTION;
    const canonicalUrl = this.absoluteUrl(seo.canonicalPath ?? this.pathOnly(currentUrl));
    const imageUrl = this.absoluteUrl(seo.imagePath ?? DEFAULT_IMAGE_PATH);
    const robots = seo.robots ?? DEFAULT_ROBOTS;
    const type = seo.type ?? 'website';

    this.title.setTitle(title);
    this.meta.updateTag({ name: 'description', content: description });
    this.meta.updateTag({ name: 'robots', content: robots });
    this.meta.updateTag({ name: 'application-name', content: APP_NAME });

    this.meta.updateTag({ property: 'og:site_name', content: APP_NAME });
    this.meta.updateTag({ property: 'og:title', content: title });
    this.meta.updateTag({ property: 'og:description', content: description });
    this.meta.updateTag({ property: 'og:type', content: type });
    this.meta.updateTag({ property: 'og:url', content: canonicalUrl });
    this.meta.updateTag({ property: 'og:image', content: imageUrl });

    this.meta.updateTag({ name: 'twitter:card', content: 'summary_large_image' });
    this.meta.updateTag({ name: 'twitter:title', content: title });
    this.meta.updateTag({ name: 'twitter:description', content: description });
    this.meta.updateTag({ name: 'twitter:image', content: imageUrl });

    this.setCanonicalLink(canonicalUrl);
    this.setStructuredData(this.resolveStructuredData(seo.structuredData));
  }

  private collectSeoData(route: ActivatedRouteSnapshot): SeoRouteData {
    const seo: SeoRouteData = {};
    let current: ActivatedRouteSnapshot | null = route;

    while (current) {
      Object.assign(seo, current.data['seo'] as SeoRouteData | undefined);
      current = current.firstChild;
    }

    return seo;
  }

  private pathOnly(url: string): string {
    const [withoutHash] = url.split('#');
    const [path] = withoutHash.split('?');
    return path || '/home';
  }

  private absoluteUrl(pathOrUrl: string): string {
    if (/^https?:\/\//i.test(pathOrUrl)) {
      return pathOrUrl;
    }

    const origin = this.siteOrigin();
    const path = pathOrUrl.startsWith('/') ? pathOrUrl : `/${pathOrUrl}`;
    return `${origin}${path}`;
  }

  private siteOrigin(): string {
    const configured = environment.siteUrl.trim().replace(/\/+$/, '');
    if (configured) {
      return configured;
    }

    return this.document.location.origin;
  }

  private setCanonicalLink(url: string): void {
    let link = this.document.querySelector<HTMLLinkElement>('link[rel="canonical"]');

    if (!link) {
      link = this.document.createElement('link');
      link.setAttribute('rel', 'canonical');
      this.document.head.appendChild(link);
    }

    link.setAttribute('href', url);
  }

  private resolveStructuredData(
    data?: Record<string, unknown>,
  ): Record<string, unknown> | undefined {
    if (!data) {
      return undefined;
    }

    const resolved = { ...data };
    for (const key of ['url', 'image']) {
      const value = resolved[key];
      if (typeof value === 'string') {
        resolved[key] = this.absoluteUrl(value);
      }
    }

    return resolved;
  }

  private setStructuredData(data?: Record<string, unknown>): void {
    const id = 'seo-structured-data';
    let script = this.document.getElementById(id) as HTMLScriptElement | null;

    if (!data) {
      script?.remove();
      return;
    }

    if (!script) {
      script = this.document.createElement('script');
      script.id = id;
      script.type = 'application/ld+json';
      this.document.head.appendChild(script);
    }

    script.text = JSON.stringify(data);
  }
}
