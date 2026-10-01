import { ComponentFixture, TestBed } from '@angular/core/testing';
import { provideRouter } from '@angular/router';

import { FaqPage } from './faq-page';

describe('FaqPage', () => {
  let fixture: ComponentFixture<FaqPage>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [FaqPage],
      providers: [provideRouter([])],
    }).compileComponents();

    fixture = TestBed.createComponent(FaqPage);
    fixture.detectChanges();
  });

  it('renders grouped FAQ questions for public visitors', () => {
    const element: HTMLElement = fixture.nativeElement;
    const text = element.textContent ?? '';
    const questions = Array.from(element.querySelectorAll('summary'));

    expect(text).toContain('Answers for evaluating SteamApp');
    expect(text).toContain('Platform Basics');
    expect(text).toContain('Checks And Matching Rules');
    expect(text).toContain('Automation And History');
    expect(text).toContain('Catalog, Accounts, And Links');
    expect(questions.length).toBe(10);
    expect(text).toContain('What does SteamApp do?');
    expect(text).toContain('Can I filter results while a check is running?');
    expect(text).toContain('What does the Automated Queue do?');
    expect(text).toContain('Does the app open external Steam or third-party links?');
  });

  it('links visitors to the About and Pricing pages from the hero', () => {
    const element = fixture.nativeElement as HTMLElement;
    const links = Array.from(element.querySelectorAll<HTMLAnchorElement>('.faq-hero__actions a'));

    expect(links.map((link) => link.textContent?.trim())).toEqual([
      'About SteamApp',
      'Compare Plans',
    ]);
    expect(links.map((link) => link.getAttribute('href'))).toEqual(['/about', '/pricing']);
  });
});
