import { HttpErrorResponse } from '@angular/common/http';
import { ChangeDetectionStrategy, Component, DestroyRef, inject, input, output, signal } from '@angular/core';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { finalize } from 'rxjs';
import { AuthService, MfaChallenge, MfaMethod } from '../../services';

@Component({
  selector: 'steam-mfa-challenge',
  changeDetection: ChangeDetectionStrategy.OnPush,
  styleUrl: './login.component.scss',
  template: `
    <form class="general-form login-form" (submit)="$event.preventDefault(); verify(code.value)">
      <h2>Verify your sign-in</h2>
      <div class="form-group">
        <label for="mfa-code">{{ method() === 'authenticator' ? 'Authenticator code' : 'Backup code' }}</label>
        <input id="mfa-code" #code required maxlength="64" autocomplete="one-time-code"
          [attr.inputmode]="method() === 'authenticator' ? 'numeric' : 'text'"
          [attr.pattern]="method() === 'authenticator' ? '[0-9]{6}' : null"
          [disabled]="pending() || expired()" />
      </div>
      <button class="login-form__submit" type="submit" [disabled]="pending() || expired()">
        {{ pending() ? 'Verifying…' : 'Verify' }}
      </button>
      @if (!expired()) {
        <button type="button" [disabled]="pending()" (click)="switchMethod(); code.value = ''">
          {{ method() === 'authenticator' ? 'Use a backup code' : 'Use an authenticator code' }}
        </button>
      }
      <button type="button" [disabled]="pending()" (click)="restart.emit()">Start sign-in again</button>
      @if (error()) {
        <p role="alert" class="error login-form__error">{{ error() }}</p>
      }
    </form>
  `,
})
export class MfaChallengeComponent {
  private readonly auth = inject(AuthService);
  private readonly destroyRef = inject(DestroyRef);
  readonly challenge = input.required<MfaChallenge>();
  readonly verified = output<void>();
  readonly restart = output<void>();
  readonly method = signal<MfaMethod>('authenticator');
  readonly pending = signal(false);
  readonly expired = signal(false);
  readonly error = signal<string | null>(null);

  switchMethod(): void {
    this.method.update(method => method === 'authenticator' ? 'recovery' : 'authenticator');
    this.error.set(null);
  }

  verify(code: string): void {
    if (this.pending() || this.expired() || !code.trim()) {
      return;
    }
    if (Date.parse(this.challenge().expiresAtUtc) <= Date.now()) {
      this.expire();
      return;
    }

    this.pending.set(true);
    this.error.set(null);
    this.auth.verifyMfa(this.challenge().challengeId, this.method(), code.trim()).pipe(
      takeUntilDestroyed(this.destroyRef),
      finalize(() => this.pending.set(false)),
    ).subscribe({
      next: () => this.verified.emit(),
      error: (error: HttpErrorResponse) => {
        if (error.status === 410) {
          this.expire();
        } else {
          this.error.set(error.status === 429
            ? 'Too many attempts. Wait a moment and try again.'
            : error.error?.title ?? 'Could not verify the code. Try again.');
        }
      },
    });
  }

  private expire(): void {
    this.expired.set(true);
    this.error.set('Verification expired. Start sign-in again.');
  }
}
