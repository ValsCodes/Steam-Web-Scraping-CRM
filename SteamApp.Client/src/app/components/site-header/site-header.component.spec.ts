import { ComponentFixture, TestBed } from '@angular/core/testing';
import { provideRouter } from '@angular/router';
import { BehaviorSubject } from 'rxjs';

import { AuthService, CurrentUser } from '../../services/auth/auth.service';
import { SiteHeaderComponent } from './site-header.component';

describe('SiteHeaderComponent', () => {
  let component: SiteHeaderComponent;
  let fixture: ComponentFixture<SiteHeaderComponent>;
  let loggedInSubject: BehaviorSubject<boolean>;
  let currentUserSubject: BehaviorSubject<CurrentUser | null>;
  let auth: jasmine.SpyObj<AuthService>;

  beforeEach(async () => {
    loggedInSubject = new BehaviorSubject<boolean>(false);
    currentUserSubject = new BehaviorSubject<CurrentUser | null>(null);
    auth = jasmine.createSpyObj<AuthService>(
      'AuthService',
      ['getTimeBeforeExpiration', 'logout'],
      {
        loggedIn$: loggedInSubject.asObservable(),
        currentUser$: currentUserSubject.asObservable(),
      },
    );
    auth.getTimeBeforeExpiration.and.returnValue(60000);

    await TestBed.configureTestingModule({
      imports: [SiteHeaderComponent],
      providers: [
        { provide: AuthService, useValue: auth },
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

  it('renders authenticated feedback navigation when signed in', () => {
    loggedInSubject.next(true);
    currentUserSubject.next({
      id: 'user-1',
      displayName: 'Test User',
      firstName: 'Test',
      lastName: 'User',
      userName: 'test-user',
      email: 'test@example.com',
      phone: null,
      clientId: null,
      scope: 'user',
      roles: ['User'],
      isAdmin: false,
    });
    fixture.detectChanges();

    const element = fixture.nativeElement as HTMLElement;
    const links = Array.from(element.querySelectorAll<HTMLAnchorElement>('.site-header__nav > a'));
    const feedbackLink = links.find((link) => link.textContent?.trim() === 'Feedback');
    const sendFeedbackLink = links.find((link) => link.textContent?.trim() === 'Send Feedback');

    expect(feedbackLink?.getAttribute('href')).toBe('/feedback');
    expect(sendFeedbackLink?.getAttribute('href')).toBe('/feedback/send');
    expect(links.some((link) => link.textContent?.trim() === 'Users')).toBeFalse();
  });

  it('renders the admin users link only for admins', () => {
    loggedInSubject.next(true);
    currentUserSubject.next({
      id: 'admin-1',
      displayName: 'Admin User',
      firstName: 'Admin',
      lastName: 'User',
      userName: 'admin-user',
      email: 'admin@example.com',
      phone: null,
      clientId: null,
      scope: 'user',
      roles: ['User', 'Admin'],
      isAdmin: true,
    });
    fixture.detectChanges();

    const element = fixture.nativeElement as HTMLElement;
    const links = Array.from(element.querySelectorAll<HTMLAnchorElement>('.site-header__nav > a'));
    const usersLink = links.find((link) => link.textContent?.trim() === 'Users');

    expect(usersLink?.getAttribute('href')).toBe('/admin/users');
  });
});
