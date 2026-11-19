import { HttpErrorResponse } from '@angular/common/http';
import {
  ChangeDetectionStrategy, Component, DestroyRef, ElementRef, OnInit,
  effect, inject, signal, viewChild,
} from '@angular/core';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { Router } from '@angular/router';
import { Observable, finalize } from 'rxjs';
import { toCanvas } from 'qrcode';
import { AuthService, MfaSetup, MfaStatus } from '../../services';

@Component({
  selector: 'steam-profile-mfa',
  changeDetection: ChangeDetectionStrategy.OnPush,
  templateUrl: './profile-mfa.component.html',
  styleUrl: './profile-page.scss',
})
export class ProfileMfaComponent implements OnInit {
  private readonly auth = inject(AuthService);
  private readonly router = inject(Router);
  private readonly destroyRef = inject(DestroyRef);
  private readonly qrCanvas = viewChild<ElementRef<HTMLCanvasElement>>('qrCanvas');
  readonly status = signal<MfaStatus | null>(null);
  readonly setup = signal<MfaSetup | null>(null);
  readonly recoveryCodes = signal<string[] | null>(null);
  readonly pending = signal(false);
  readonly error = signal<string | null>(null);
  readonly qrError = signal(false);
  readonly requiresSignIn = signal(false);

  constructor() {
    effect(() => {
      const canvas = this.qrCanvas()?.nativeElement;
      const setup = this.setup();
      if (canvas && setup) {
        void toCanvas(canvas, setup.otpAuthUri, { width: 224 }).catch(() => {
          if (this.setup() === setup) {
            this.qrError.set(true);
          }
        });
      }
    });
  }

  ngOnInit(): void {
    this.load();
  }

  load(): void {
    this.run(this.auth.getMfaStatus(), status => this.status.set(status));
  }

  beginSetup(): void {
    this.qrError.set(false);
    this.run(this.auth.setupMfa(), setup => this.setup.set(setup));
  }

  enable(code: string): void {
    const setup = this.setup();
    if (setup && /^[0-9]{6}$/.test(code)) {
      this.run(this.auth.enableMfa(setup.setupId, code), response => {
        this.setup.set(null);
        this.showRecoveryCodes(response.recoveryCodes);
      });
    }
  }

  replaceCodes(): void {
    this.run(this.auth.replaceMfaRecoveryCodes(), response => this.showRecoveryCodes(response.recoveryCodes));
  }

  disable(): void {
    this.run(this.auth.disableMfa(), () => {
      this.recoveryCodes.set(null);
      this.status.set({ enabled: false, recoveryCodesRemaining: 0 });
    });
  }

  signInAgain(): void {
    this.auth.logout();
    void this.router.navigate(['/login'], { replaceUrl: true, queryParams: { returnTo: 'profile' } });
  }

  private showRecoveryCodes(codes: string[]): void {
    this.recoveryCodes.set(codes);
    this.status.set({ enabled: true, recoveryCodesRemaining: codes.length });
  }

  private run<T>(request: Observable<T>, onSuccess: (result: T) => void): void {
    if (this.pending()) {
      return;
    }

    this.pending.set(true);
    this.error.set(null);
    this.requiresSignIn.set(false);
    request.pipe(
      takeUntilDestroyed(this.destroyRef),
      finalize(() => this.pending.set(false)),
    ).subscribe({
      next: onSuccess,
      error: (error: HttpErrorResponse) => {
        if (error.status === 403 && error.error?.type === 'urn:steamapp:mfa:recent-sign-in-required') {
          this.requiresSignIn.set(true);
          this.setup.set(null);
        } else if (error.status === 410 || error.status === 409) {
          this.setup.set(null);
        }
        this.error.set(error.status === 429
          ? error.error?.detail ?? 'Too many requests. Wait a moment and try again.'
          : error.error?.title ?? 'Could not update authenticator settings. Try again.');
      },
    });
  }
}
