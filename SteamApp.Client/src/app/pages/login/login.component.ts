import { CommonModule } from "@angular/common";
import { ChangeDetectionStrategy, ChangeDetectorRef, Component } from "@angular/core";
import { FormControl, FormGroup, ReactiveFormsModule } from "@angular/forms";
import { AuthService } from "../../services";
import { Router } from "@angular/router";
import { finalize } from "rxjs";

@Component({
  selector: 'steam-login',
  standalone: true,
  changeDetection: ChangeDetectionStrategy.OnPush,
  imports: [CommonModule, ReactiveFormsModule],
  templateUrl: './login.component.html',
})
export class LoginComponent {
  loginForm = new FormGroup({
    clientId: new FormControl('', { nonNullable: true }),
    clientSecret: new FormControl('', { nonNullable: true }),
  });

  isSubmitting = false;
  error: string | null = null;

  constructor(
    private readonly auth: AuthService,
    private readonly router: Router,
    private readonly cdr: ChangeDetectorRef
  ) {}

  onSubmit(): void {
  if (this.isSubmitting || this.loginForm.invalid) {
    return;
  }

  this.isSubmitting = true;
  this.error = null;
  this.cdr.markForCheck();

  const { clientId, clientSecret } = this.loginForm.getRawValue();

  this.auth.login(clientId, clientSecret)
    .pipe(
      finalize(() => {
        this.isSubmitting = false;
        this.cdr.markForCheck();
      })
    )
    .subscribe({
      next: () => {
        this.router.navigateByUrl('/games', { replaceUrl: true });
      },
      error: () => {
        this.error = 'Login failed';
      }
    });
}
}
