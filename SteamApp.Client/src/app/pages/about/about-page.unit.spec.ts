import { ComponentFixture, TestBed } from '@angular/core/testing';
import { provideRouter } from '@angular/router';

import { AboutPage } from './about-page';

describe('AboutPage', () => {
  let fixture: ComponentFixture<AboutPage>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [AboutPage],
      providers: [provideRouter([])],
    }).compileComponents();

    fixture = TestBed.createComponent(AboutPage);
    fixture.detectChanges();
  });

  it('renders the public product story and primary sections', () => {
    const element: HTMLElement = fixture.nativeElement;
    const text = element.textContent ?? '';

    expect(text).toContain('About Steam Web Scraping CRM');
    expect(text).toContain('Reduce the manual work behind Steam market monitoring');
    expect(text).toContain('What It Manages');
    expect(text).toContain('Who It Helps');
    expect(text).toContain('Capabilities');
    expect(text).toContain('Steam market sources');
    expect(text).toContain('Operators');
    expect(text).toContain('operational routines');
  });

  it('links visitors to pricing and login from the hero', () => {
    const element = fixture.nativeElement as HTMLElement;
    const links = Array.from(element.querySelectorAll<HTMLAnchorElement>('.about-hero__actions a'));

    expect(links.map((link) => link.textContent?.trim())).toEqual(['View Pricing', 'Sign In']);
    expect(links.map((link) => link.getAttribute('href'))).toEqual(['/pricing', '/login']);
  });
});
