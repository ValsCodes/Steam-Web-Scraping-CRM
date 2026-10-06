import { ComponentFixture, TestBed } from '@angular/core/testing';
import { provideRouter } from '@angular/router';

import { HomePage } from './home-page';

describe('HomePage', () => {
  let fixture: ComponentFixture<HomePage>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [HomePage],
      providers: [provideRouter([])],
    }).compileComponents();

    fixture = TestBed.createComponent(HomePage);
    fixture.detectChanges();
  });

  it('presents checks and queue automation as the primary workflows', () => {
    const element: HTMLElement = fixture.nativeElement;
    const text = element.textContent ?? '';

    expect(text).toContain('Steam market intelligence and workflow automation');
    expect(text).toContain('Manual Checks');
    expect(text).toContain('Automated Queue');
    expect(text).toContain('preset combinations');
    expect(text).not.toContain('Web Scraper');
    expect(text).not.toContain('Pixels');
  });

  it('links the hero to active manual-check and queue routes', () => {
    const element: HTMLElement = fixture.nativeElement;
    const links = Array.from(element.querySelectorAll<HTMLAnchorElement>('.hero__actions a'));

    expect(links.map((link) => link.textContent?.trim())).toEqual([
      'Run a Manual Check',
      'Open Automated Queue',
    ]);
    expect(links.map((link) => link.getAttribute('href'))).toEqual([
      '/manual-checks',
      '/automatic-queue',
    ]);
  });
});
