import { ComponentFixture, TestBed } from '@angular/core/testing';
import { provideHttpClient } from '@angular/common/http';
import { provideRouter } from '@angular/router';

import { SiteFooter } from './site-footer';

describe('SiteFooter', () => {
  let component: SiteFooter;
  let fixture: ComponentFixture<SiteFooter>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [SiteFooter],
      providers: [
        provideHttpClient(),
        provideRouter([]),
      ],
    })
    .compileComponents();

    fixture = TestBed.createComponent(SiteFooter);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });

  it('renders public footer links', () => {
    const element = fixture.nativeElement as HTMLElement;
    const links = Array.from(element.querySelectorAll<HTMLAnchorElement>('.site-footer__nav a'));

    expect(links.map((link) => link.textContent?.trim())).toEqual([
      'Home',
      'Pricing',
      'About',
      'FAQ',
      'External Link Disclosure',
    ]);
    expect(links.map((link) => link.getAttribute('href'))).toEqual([
      '/home',
      '/pricing',
      '/about',
      '/faq',
      '/external-link-disclosure',
    ]);
  });
});
