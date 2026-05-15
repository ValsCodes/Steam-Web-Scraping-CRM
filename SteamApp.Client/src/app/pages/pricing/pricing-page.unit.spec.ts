import { ComponentFixture, TestBed } from '@angular/core/testing';
import { provideRouter } from '@angular/router';

import { PricingPage } from './pricing-page';

describe('PricingPage', () => {
  let fixture: ComponentFixture<PricingPage>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [PricingPage],
      providers: [provideRouter([])],
    }).compileComponents();

    fixture = TestBed.createComponent(PricingPage);
    fixture.detectChanges();
  });

  it('renders the subscription tiers with benefits and login calls to action', () => {
    const element: HTMLElement = fixture.nativeElement;
    const text = element.textContent ?? '';

    expect(text).toContain('Free Tier');
    expect(text).toContain('Tier 1');
    expect(text).toContain('Tier 3');
    expect(text).toContain('Core catalog setup');
    expect(text).toContain('Web scraper and public API workflows');
    expect(text).toContain('Pixel validation support');
    expect(text).toContain('Recommended');

    const links = Array.from(element.querySelectorAll<HTMLAnchorElement>('.plan__cta'));

    expect(links.map((link) => link.textContent?.trim())).toEqual([
      'Start Free',
      'Choose Tier 1',
      'Request Tier 3',
    ]);
    expect(links.map((link) => link.getAttribute('href'))).toEqual(['/login', '/login', '/login']);
  });
});
