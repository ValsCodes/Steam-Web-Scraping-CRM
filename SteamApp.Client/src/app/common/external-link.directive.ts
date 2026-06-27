import { Directive, HostBinding, HostListener, Input, OnChanges, inject } from '@angular/core';
import { Router } from '@angular/router';

import { ExternalLinkDisclosureService } from '../services/external-link-disclosure.service';
import { externalUrlWarning, openableExternalUrl } from './url-security';

@Directive({
  selector: 'a[steamExternalLink]',
  standalone: true,
})
export class ExternalLinkDirective implements OnChanges {
  private readonly disclosure = inject(ExternalLinkDisclosureService);
  private readonly router = inject(Router);

  @Input('steamExternalLink') targetUrl: string | null | undefined;
  @Input() steamExternalLinkReturnUrl: string | null | undefined;

  @HostBinding('attr.href') href: string | null = null;
  @HostBinding('attr.target') target: string | null = null;
  @HostBinding('attr.rel') rel: string | null = null;
  @HostBinding('attr.referrerpolicy') referrerPolicy: string | null = null;
  @HostBinding('attr.title') title: string | null = null;
  @HostBinding('attr.aria-disabled') ariaDisabled: string | null = null;
  @HostBinding('class.external-link--blocked') isBlocked = false;
  @HostBinding('class.external-link--warning') hasWarning = false;

  ngOnChanges(): void {
    this.updateLinkAttributes();
  }

  @HostListener('click', ['$event'])
  onClick(event: MouseEvent): void {
    event.preventDefault();

    const result = this.disclosure.openTrustedUrl(
      this.targetUrl,
      this.steamExternalLinkReturnUrl ?? this.router.url,
    );

    if (result !== 'opened') {
      this.updateLinkAttributes();
    }
  }

  private updateLinkAttributes(): void {
    const openableUrl = openableExternalUrl(this.targetUrl);
    const warning = externalUrlWarning(this.targetUrl);

    this.isBlocked = openableUrl === null;
    this.ariaDisabled = this.isBlocked ? 'true' : null;
    this.hasWarning = openableUrl !== null && warning !== null;
    this.title = warning;

    if (!openableUrl) {
      this.href = null;
      this.target = null;
      this.rel = null;
      this.referrerPolicy = null;
      return;
    }

    if (!this.disclosure.hasAccepted()) {
      this.href = this.disclosure.getDisclosureUrl(
        openableUrl,
        this.steamExternalLinkReturnUrl ?? this.router.url,
      );
      this.target = null;
      this.rel = null;
      this.referrerPolicy = null;
      return;
    }

    this.href = openableUrl;
    this.target = '_blank';
    this.rel = 'noopener noreferrer';
    this.referrerPolicy = 'no-referrer';
  }
}
