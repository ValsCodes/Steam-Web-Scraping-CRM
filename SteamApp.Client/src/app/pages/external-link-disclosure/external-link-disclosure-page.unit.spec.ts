import { ComponentFixture, TestBed } from '@angular/core/testing';
import { ActivatedRoute, Router, convertToParamMap } from '@angular/router';

import { EXTERNAL_LINK_DISCLOSURE_STORAGE_KEY } from '../../services/external-link-disclosure.service';
import { ExternalLinkDisclosurePage } from './external-link-disclosure-page';

describe('ExternalLinkDisclosurePage', () => {
  let fixture: ComponentFixture<ExternalLinkDisclosurePage>;
  let component: ExternalLinkDisclosurePage;
  let router: { navigateByUrl: jasmine.Spy; url: string };
  let queryParams: Record<string, string>;

  beforeEach(async () => {
    localStorage.removeItem(EXTERNAL_LINK_DISCLOSURE_STORAGE_KEY);
    queryParams = {
      target: 'https://steamcommunity.com/market/',
      returnTo: '/manual-mode-v2',
    };

    router = {
      url: '/external-link-disclosure',
      navigateByUrl: jasmine.createSpy('navigateByUrl').and.resolveTo(true),
    };

    await TestBed.configureTestingModule({
      imports: [ExternalLinkDisclosurePage],
      providers: [
        {
          provide: ActivatedRoute,
          useFactory: () => ({
            snapshot: {
              queryParamMap: convertToParamMap(queryParams),
            },
          }),
        },
        { provide: Router, useValue: router },
      ],
    }).compileComponents();
  });

  afterEach(() => {
    localStorage.removeItem(EXTERNAL_LINK_DISCLOSURE_STORAGE_KEY);
  });

  function createComponent(): void {
    fixture = TestBed.createComponent(ExternalLinkDisclosurePage);
    component = fixture.componentInstance;
    fixture.detectChanges();
  }

  it('renders the disclosure copy and safe destination', () => {
    createComponent();

    const text = fixture.nativeElement.textContent as string;

    expect(text).toContain('External Link Disclosure');
    expect(text).toContain('SteamApp can open links to third-party websites');
    expect(text).toContain('https://steamcommunity.com/market/');
    expect(component.openableTargetUrl).toBe('https://steamcommunity.com/market/');
  });

  it('warns for untrusted web targets without blocking them', () => {
    queryParams = {
      target: 'https://evil.example/market',
      returnTo: '/manual-mode-v2',
    };

    createComponent();

    expect(component.openableTargetUrl).toBe('https://evil.example/market');
    expect(component.targetWarning).toContain('Warning');
    expect(fixture.nativeElement.textContent).toContain('Warning');
  });

  it('warns for custom scheme targets without blocking them', () => {
    queryParams = {
      target: 'javascript:alert(1)',
      returnTo: '/manual-mode-v2',
    };

    createComponent();

    expect(component.openableTargetUrl).toBe('javascript:alert(1)');
    expect(component.targetWarning).toContain('Warning');
    expect(fixture.nativeElement.textContent).toContain('Accept and Open Link');
  });

  it('blocks empty targets from being opened', () => {
    queryParams = {
      target: ' ',
      returnTo: '/manual-mode-v2',
    };

    createComponent();

    expect(component.openableTargetUrl).toBeNull();
    expect(fixture.nativeElement.textContent).toContain('cannot be opened');
  });

  it('requires checkbox acceptance before persisting agreement', () => {
    createComponent();

    component.acceptWithoutOpening();

    expect(component.acceptanceError).toContain('Confirm');
    expect(localStorage.getItem(EXTERNAL_LINK_DISCLOSURE_STORAGE_KEY)).toBeNull();
    expect(router.navigateByUrl).not.toHaveBeenCalled();
  });

  it('accepts and opens a safe target, then returns to the previous page', () => {
    const openSpy = spyOn(window, 'open').and.returnValue(null);
    createComponent();
    component.acceptedControl.setValue(true);

    component.acceptAndOpenTarget();

    expect(localStorage.getItem(EXTERNAL_LINK_DISCLOSURE_STORAGE_KEY)).toBe('accepted');
    expect(openSpy).toHaveBeenCalledWith(
      'https://steamcommunity.com/market/',
      '_blank',
      'noopener,noreferrer',
    );
    expect(router.navigateByUrl).toHaveBeenCalledWith('/manual-mode-v2');
  });

  it('cancels back to the normalized return URL', () => {
    createComponent();

    component.cancel();

    expect(router.navigateByUrl).toHaveBeenCalledWith('/manual-mode-v2');
  });
});
