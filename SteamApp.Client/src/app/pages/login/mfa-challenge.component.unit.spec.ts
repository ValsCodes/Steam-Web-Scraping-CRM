import { HttpErrorResponse } from '@angular/common/http';
import { TestBed } from '@angular/core/testing';
import { of, throwError } from 'rxjs';
import { AuthService } from '../../services';
import { MfaChallengeComponent } from './mfa-challenge.component';

describe('MfaChallengeComponent', () => {
  const verifyMfa = jasmine.createSpy('verifyMfa');

  beforeEach(() => {
    verifyMfa.calls.reset();
    TestBed.configureTestingModule({
      imports: [MfaChallengeComponent],
      providers: [{ provide: AuthService, useValue: { verifyMfa } }],
    });
  });

  it('lets the user retry with a backup code and completes sign-in', () => {
    verifyMfa.and.returnValues(
      throwError(() => new HttpErrorResponse({ status: 400, error: { title: 'Incorrect code' } })),
      of({ token: 'session' }),
    );
    const fixture = TestBed.createComponent(MfaChallengeComponent);
    fixture.componentRef.setInput('challenge', {
      requiresMfa: true,
      challengeId: 'pending',
      expiresAtUtc: new Date(Date.now() + 300_000).toISOString(),
    });
    fixture.detectChanges();
    const component = fixture.componentInstance;
    const verified = jasmine.createSpy('verified');
    component.verified.subscribe(verified);

    component.verify('123456');
    expect(component.error()).toBe('Incorrect code');
    expect(verified).not.toHaveBeenCalled();

    component.switchMethod();
    component.verify('backup');
    expect(verifyMfa).toHaveBeenCalledWith('pending', 'recovery', 'backup');
    expect(verified).toHaveBeenCalledTimes(1);
  });

  it('requires a fresh sign-in after the server expires the challenge', () => {
    verifyMfa.and.returnValue(throwError(() => new HttpErrorResponse({ status: 410 })));
    const fixture = TestBed.createComponent(MfaChallengeComponent);
    fixture.componentRef.setInput('challenge', {
      requiresMfa: true,
      challengeId: 'pending',
      expiresAtUtc: new Date(Date.now() + 300_000).toISOString(),
    });
    fixture.detectChanges();
    const component = fixture.componentInstance;
    const restart = jasmine.createSpy('restart');
    component.restart.subscribe(restart);

    component.verify('123456');
    component.verify('654321');
    fixture.detectChanges();
    fixture.nativeElement.querySelector('button[type="button"]').click();

    expect(component.expired()).toBeTrue();
    expect(verifyMfa).toHaveBeenCalledTimes(1);
    expect(restart).toHaveBeenCalledTimes(1);
  });
});
