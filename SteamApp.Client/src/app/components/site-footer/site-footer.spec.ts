import { ComponentFixture, TestBed } from '@angular/core/testing';
import { provideRouter } from '@angular/router';
import { BehaviorSubject } from 'rxjs';

import { AuthService } from '../../services/auth/auth.service';
import { SiteFooter } from './site-footer';

describe('SiteFooter', () => {
  let component: SiteFooter;
  let fixture: ComponentFixture<SiteFooter>;
  let loggedInSubject: BehaviorSubject<boolean>;

  beforeEach(async () => {
    loggedInSubject = new BehaviorSubject<boolean>(false);

    await TestBed.configureTestingModule({
      imports: [SiteFooter],
      providers: [
        { provide: AuthService, useValue: { loggedIn$: loggedInSubject.asObservable() } },
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

  it('renders authenticated feedback footer link when signed in', () => {
    loggedInSubject.next(true);
    fixture.detectChanges();

    const element = fixture.nativeElement as HTMLElement;
    const links = Array.from(element.querySelectorAll<HTMLAnchorElement>('.site-footer__nav a'));
    const feedbackLink = links.find((link) => link.textContent?.trim() === 'Feedback');
    const sendFeedbackLink = links.find((link) => link.textContent?.trim() === 'Send Feedback');

    expect(feedbackLink?.getAttribute('href')).toBe('/feedback');
    expect(sendFeedbackLink?.getAttribute('href')).toBe('/feedback/send');
  });
});
