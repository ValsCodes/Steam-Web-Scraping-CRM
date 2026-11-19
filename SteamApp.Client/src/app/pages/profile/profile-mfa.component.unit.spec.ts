import { HttpErrorResponse } from '@angular/common/http';
import { TestBed } from '@angular/core/testing';
import { Router } from '@angular/router';
import { of, throwError } from 'rxjs';
import { AuthService } from '../../services';
import { ProfileMfaComponent } from './profile-mfa.component';

describe('ProfileMfaComponent', () => {
  let auth: jasmine.SpyObj<AuthService>;
  let router: jasmine.SpyObj<Router>;

  beforeEach(() => {
    auth = jasmine.createSpyObj<AuthService>('AuthService', ['getMfaStatus', 'setupMfa', 'enableMfa', 'logout']);
    auth.getMfaStatus.and.returnValue(of({ enabled: false, recoveryCodesRemaining: 0 }));
    router = jasmine.createSpyObj<Router>('Router', ['navigate']);
    router.navigate.and.resolveTo(true);
    TestBed.configureTestingModule({
      imports: [ProfileMfaComponent],
      providers: [
        { provide: AuthService, useValue: auth },
        { provide: Router, useValue: router },
      ],
    });
  });

  it('renders the QR code and manual key, then shows the backup codes once after confirmation', async () => {
    auth.setupMfa.and.returnValue(of({
      setupId: 'setup',
      expiresAtUtc: new Date(Date.now() + 300_000).toISOString(),
      manualKey: 'MANUALKEY',
      otpAuthUri: 'otpauth://totp/SteamApp:test?secret=MANUALKEY&issuer=SteamApp',
    }));
    auth.enableMfa.and.returnValue(of({ session: { token: 'new-token' }, recoveryCodes: ['backup-1', 'backup-2'] }));
    const fixture = TestBed.createComponent(ProfileMfaComponent);
    fixture.detectChanges();
    const component = fixture.componentInstance;

    component.beginSetup();
    fixture.detectChanges();
    await fixture.whenStable();
    expect(fixture.nativeElement.querySelector('canvas').width).toBe(224);
    expect(fixture.nativeElement.textContent).toContain('MANUALKEY');

    component.enable('123456');
    fixture.detectChanges();
    expect(auth.enableMfa).toHaveBeenCalledWith('setup', '123456');
    expect(fixture.nativeElement.textContent).not.toContain('MANUALKEY');
    expect(fixture.nativeElement.textContent).toContain('backup-1');

    fixture.nativeElement.querySelector('button').click();
    fixture.detectChanges();
    expect(fixture.nativeElement.textContent).not.toContain('backup-1');
    expect(fixture.nativeElement.textContent).toContain('Backup codes remaining: 2');
  });

  it('sends an older session through sign-in and back to Profile', () => {
    auth.setupMfa.and.returnValue(throwError(() => new HttpErrorResponse({
      status: 403,
      error: { type: 'urn:steamapp:mfa:recent-sign-in-required' },
    })));
    const fixture = TestBed.createComponent(ProfileMfaComponent);
    fixture.detectChanges();
    const component = fixture.componentInstance;

    component.beginSetup();
    expect(component.requiresSignIn()).toBeTrue();
    component.signInAgain();

    expect(auth.logout).toHaveBeenCalled();
    expect(router.navigate).toHaveBeenCalledWith(['/login'], {
      replaceUrl: true,
      queryParams: { returnTo: 'profile' },
    });
  });
});
