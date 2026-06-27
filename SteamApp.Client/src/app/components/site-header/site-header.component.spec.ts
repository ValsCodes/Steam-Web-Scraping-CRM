import { ComponentFixture, TestBed } from '@angular/core/testing';
import { provideHttpClient } from '@angular/common/http';
import { provideRouter } from '@angular/router';

import { SiteHeaderComponent } from './site-header.component';

describe('SiteHeaderComponent', () => {
  let component: SiteHeaderComponent;
  let fixture: ComponentFixture<SiteHeaderComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [SiteHeaderComponent],
      providers: [
        provideHttpClient(),
        provideRouter([]),
      ],
    })
    .compileComponents();

    fixture = TestBed.createComponent(SiteHeaderComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });

  it('renders public navigation links for unauthenticated visitors', () => {
    const element = fixture.nativeElement as HTMLElement;
    const links = Array.from(element.querySelectorAll<HTMLAnchorElement>('.site-header__nav > a'));

    expect(links.map((link) => link.textContent?.trim())).toEqual([
      'Home',
      'Pricing',
      'About',
      'FAQ',
    ]);
    expect(links.map((link) => link.getAttribute('href'))).toEqual([
      '/home',
      '/pricing',
      '/about',
      '/faq',
    ]);
  });
});
