import { Component } from '@angular/core';
import { ComponentFixture, TestBed } from '@angular/core/testing';
import { Router } from '@angular/router';

import { ExternalLinkDirective } from './external-link.directive';
import { EXTERNAL_LINK_DISCLOSURE_STORAGE_KEY } from '../services/external-link-disclosure.service';

@Component({
  standalone: true,
  imports: [ExternalLinkDirective],
  template: `
    <a [steamExternalLink]="url" steamExternalLinkReturnUrl="/games">Open</a>
  `,
})
class ExternalLinkHostComponent {
  url: string | null = 'https://backpack.tf/stats';
}

describe('ExternalLinkDirective', () => {
  let fixture: ComponentFixture<ExternalLinkHostComponent>;
  let component: ExternalLinkHostComponent;
  let router: { url: string; navigateByUrl: jasmine.Spy };

  beforeEach(async () => {
    localStorage.removeItem(EXTERNAL_LINK_DISCLOSURE_STORAGE_KEY);

    router = {
      url: '/games',
      navigateByUrl: jasmine.createSpy('navigateByUrl').and.resolveTo(true),
    };

    await TestBed.configureTestingModule({
      imports: [ExternalLinkHostComponent],
      providers: [{ provide: Router, useValue: router }],
    }).compileComponents();

    fixture = TestBed.createComponent(ExternalLinkHostComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  afterEach(() => {
    localStorage.removeItem(EXTERNAL_LINK_DISCLOSURE_STORAGE_KEY);
  });

  it('points unaccepted trusted links at the disclosure page', () => {
    const anchor = fixture.nativeElement.querySelector('a') as HTMLAnchorElement;

    expect(anchor.getAttribute('href')).toContain('/external-link-disclosure?');
    expect(anchor.getAttribute('target')).toBeNull();
  });

  it('routes to disclosure when a trusted link is clicked before acceptance', () => {
    const anchor = fixture.nativeElement.querySelector('a') as HTMLAnchorElement;

    anchor.dispatchEvent(new MouseEvent('click', { bubbles: true, cancelable: true }));

    expect(router.navigateByUrl).toHaveBeenCalled();
    expect(router.navigateByUrl.calls.mostRecent().args[0]).toContain('/external-link-disclosure?');
  });

  it('opens trusted links after acceptance', () => {
    const openSpy = spyOn(window, 'open').and.returnValue(null);
    localStorage.setItem(EXTERNAL_LINK_DISCLOSURE_STORAGE_KEY, 'accepted');
    const anchor = fixture.nativeElement.querySelector('a') as HTMLAnchorElement;

    anchor.dispatchEvent(new MouseEvent('click', { bubbles: true, cancelable: true }));

    expect(openSpy).toHaveBeenCalledWith(
      'https://backpack.tf/stats',
      '_blank',
      'noopener,noreferrer',
    );
  });

  it('keeps untrusted web URLs openable with a warning tooltip', () => {
    component.url = 'https://evil.example/market';
    fixture.detectChanges();

    const anchor = fixture.nativeElement.querySelector('a') as HTMLAnchorElement;

    expect(anchor.getAttribute('href')).toContain('/external-link-disclosure?');
    expect(anchor.getAttribute('title')).toContain('Warning');
    expect(anchor.getAttribute('aria-disabled')).toBeNull();
  });

  it('keeps custom scheme URLs openable with a warning tooltip', () => {
    component.url = 'javascript:alert(1)';
    fixture.detectChanges();

    const anchor = fixture.nativeElement.querySelector('a') as HTMLAnchorElement;

    expect(anchor.getAttribute('href')).toContain('/external-link-disclosure?');
    expect(anchor.getAttribute('title')).toContain('Warning');
    expect(anchor.getAttribute('aria-disabled')).toBeNull();
  });

  it('removes the href for empty targets', () => {
    component.url = ' ';
    fixture.detectChanges();

    const anchor = fixture.nativeElement.querySelector('a') as HTMLAnchorElement;

    expect(anchor.getAttribute('href')).toBeNull();
    expect(anchor.getAttribute('aria-disabled')).toBe('true');
  });
});
