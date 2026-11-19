import { HttpErrorResponse } from '@angular/common/http';
import { ChangeDetectionStrategy, Component, DestroyRef, OnInit, inject, signal } from '@angular/core';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { ActivatedRoute, Router, RouterLink } from '@angular/router';
import { finalize } from 'rxjs';
import { AuthService, MfaChallenge } from '../../services';
import { MfaChallengeComponent } from './mfa-challenge.component';

@Component({
  selector: 'steam-external-sign-in',
  imports: [RouterLink, MfaChallengeComponent],
  changeDetection: ChangeDetectionStrategy.OnPush,
  styleUrl: './login.component.scss',
  template: `
    @if (mfaChallenge(); as challenge) {
      <steam-mfa-challenge [challenge]="challenge" (verified)="finishSignIn()" (restart)="restart()" />
    } @else {
    <section class="general-form login-form">
      <h2>Complete sign-in</h2>
      @if (needsEmail()) {
        @if (codeSent()) {
          <form (submit)="$event.preventDefault(); complete(undefined, code.value)">
            <label for="external-code">Verification code</label>
            <input id="external-code" #code inputmode="numeric" pattern="[0-9]{6}" required
              autocomplete="one-time-code" maxlength="6" />
            <button class="login-form__submit" [disabled]="pending()">Verify email</button>
          </form>
        } @else {
          <form (submit)="$event.preventDefault(); complete(email.value)">
            <label for="external-email">Email address</label>
            <input id="external-email" #email type="email" autocomplete="email" required maxlength="256" />
            <button class="login-form__submit" [disabled]="pending()">Send verification code</button>
          </form>
        }
      }
      @if (pending()) {
        <p role="status">Working…</p>
      }
      @if (error()) {
        <p class="error login-form__error" role="alert">{{ error() }}</p>
      }
      <a [routerLink]="auth.isLoggedIn() ? '/profile' : '/login'" [queryParams]="returnToProfile ? { returnTo: 'profile' } : null">
        Return to {{ auth.isLoggedIn() ? 'Profile' : 'login' }}
      </a>
    </section>
    }
  `,
})
export class ExternalSignInComponent implements OnInit {
  readonly auth = inject(AuthService);
  private readonly route = inject(ActivatedRoute);
  private readonly router = inject(Router);
  private readonly destroyRef = inject(DestroyRef);

  readonly needsEmail = signal(false);
  readonly codeSent = signal(false);
  readonly pending = signal(false);
  readonly error = signal<string | null>(null);
  readonly mfaChallenge = signal<MfaChallenge | null>(null);
  readonly returnToProfile = this.auth.takeExternalProfileReturn();

  ngOnInit(): void {
    if (this.route.snapshot.queryParamMap.has('failed')) {
      this.error.set('Provider sign-in could not be completed. Start again.');
      return;
    }
    this.complete();
  }

  complete(email?: string, code?: string): void {
    if (this.pending()) {
      return;
    }
    this.pending.set(true);
    this.error.set(null);

    this.auth.completeExternalSignIn(email, code).pipe(
      takeUntilDestroyed(this.destroyRef),
      finalize(() => this.pending.set(false)),
    ).subscribe({
      next: response => {
        if (response && 'requiresEmail' in response) {
          this.needsEmail.set(true);
          this.codeSent.set(response.codeSent);
          return;
        }
        if (response && 'requiresMfa' in response) {
          this.mfaChallenge.set(response);
          return;
        }
        if (response) {
          this.finishSignIn();
        } else {
          void this.router.navigateByUrl('/profile', { replaceUrl: true });
        }
      },
      error: (error: HttpErrorResponse) => {
        if ([403, 409, 410].includes(error.status)) {
          this.needsEmail.set(false);
        }
        this.error.set(error.error?.title ?? 'Could not complete provider sign-in. Try again.');
      },
    });
  }

  finishSignIn(): void {
    this.mfaChallenge.set(null);
    void this.router.navigateByUrl(this.returnToProfile ? '/profile' : '/games', { replaceUrl: true });
  }

  restart(): void {
    this.mfaChallenge.set(null);
    void this.router.navigate(['/login'], {
      replaceUrl: true,
      queryParams: this.returnToProfile ? { returnTo: 'profile' } : null,
    });
  }
}
