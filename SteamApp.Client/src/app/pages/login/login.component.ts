import { CommonModule } from "@angular/common";
import { HttpErrorResponse } from "@angular/common/http";
import { ChangeDetectionStrategy, ChangeDetectorRef, Component } from "@angular/core";
import { FormControl, FormGroup, ReactiveFormsModule } from "@angular/forms";
import { AuthService } from "../../services";
import { Router } from "@angular/router";
import { finalize, Observable } from "rxjs";

type AuthMode = 'login' | 'register';

interface PasswordRequirement {
  label: string;
  isMet: boolean;
}

@Component({
  selector: 'steam-login',
  standalone: true,
  changeDetection: ChangeDetectionStrategy.OnPush,
  imports: [CommonModule, ReactiveFormsModule],
  templateUrl: './login.component.html',
  styleUrl: './login.component.scss',
})
export class LoginComponent {
  loginForm = new FormGroup({
    firstName: new FormControl('', { nonNullable: true }),
    lastName: new FormControl('', { nonNullable: true }),
    emailOrUserName: new FormControl('', { nonNullable: true }),
    email: new FormControl('', { nonNullable: true }),
    phone: new FormControl('', { nonNullable: true }),
    userName: new FormControl('', { nonNullable: true }),
    password: new FormControl('', { nonNullable: true }),
    confirmPassword: new FormControl('', { nonNullable: true }),
  });

  mode: AuthMode = 'login';
  isSubmitting = false;
  error: string | null = null;

  constructor(
    private readonly auth: AuthService,
    private readonly router: Router,
    private readonly cdr: ChangeDetectorRef
  ) {}

  get isRegisterMode(): boolean {
    return this.mode === 'register';
  }

  get pageTitle(): string {
    return this.isRegisterMode ? 'Create account' : 'Login';
  }

  get submitLabel(): string {
    if (this.isSubmitting) {
      return 'Working...';
    }

    if (this.isRegisterMode) {
      return 'Create account';
    }

    return 'Log in';
  }

  get passwordRequirements(): PasswordRequirement[] {
    const password = this.loginForm.controls.password.value;

    return [
      { label: 'At least 8 characters', isMet: password.length >= 8 },
      { label: 'One uppercase letter', isMet: /[A-Z]/.test(password) },
      { label: 'One lowercase letter', isMet: /[a-z]/.test(password) },
      { label: 'One number', isMet: /\d/.test(password) },
    ];
  }

  setMode(mode: AuthMode): void {
    this.mode = mode;
    this.error = null;
    this.loginForm.reset();
  }

  onSubmit(): void {
    if (this.isSubmitting) {
      return;
    }

    this.error = null;
    const request = this.createSubmitRequest();

    if (!request) {
      this.cdr.markForCheck();
      return;
    }

    this.isSubmitting = true;
    this.cdr.markForCheck();

    request
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
        error: error => {
          this.error = this.getErrorMessage(error);
          this.cdr.markForCheck();
        }
      });
  }

  private createSubmitRequest(): Observable<unknown> | null {
    if (this.isRegisterMode) {
      return this.createRegisterRequest();
    }

    return this.createLoginRequest();
  }

  private createLoginRequest() {
    const { emailOrUserName, password } = this.loginForm.getRawValue();

    if (!emailOrUserName.trim() || !password) {
      this.error = 'Enter your username and password';
      return null;
    }

    return this.auth.login(emailOrUserName.trim(), password);
  }

  private createRegisterRequest() {
    const {
      firstName,
      lastName,
      email,
      phone,
      userName,
      password,
      confirmPassword,
    } = this.loginForm.getRawValue();

    if (!email.trim() || !password) {
      this.error = 'Enter your email and password';
      return null;
    }

    const missingRequirements = this.passwordRequirements
      .filter(requirement => !requirement.isMet)
      .map(requirement => requirement.label.toLowerCase());

    if (missingRequirements.length > 0) {
      this.error = `Password must include ${missingRequirements.join(', ')}`;
      return null;
    }

    if (password !== confirmPassword) {
      this.error = 'Passwords do not match';
      return null;
    }

    return this.auth.register(
      email.trim(),
      userName.trim() || null,
      password,
      firstName.trim() || null,
      lastName.trim() || null,
      phone.trim() || null,
    );
  }

  private getErrorMessage(error: unknown): string {
    const fallback = this.isRegisterMode
      ? 'Account creation failed'
      : 'Login failed';

    if (!(error instanceof HttpErrorResponse)) {
      return fallback;
    }

    const validationMessage = this.getValidationMessage(error.error);
    if (validationMessage) {
      return validationMessage;
    }

    if (typeof error.error === 'string' && error.error.trim()) {
      return error.error;
    }

    if (this.isRecord(error.error) && typeof error.error['title'] === 'string') {
      return error.error['title'];
    }

    return fallback;
  }

  private getValidationMessage(errorBody: unknown): string | null {
    if (!this.isRecord(errorBody) || !this.isRecord(errorBody['errors'])) {
      return null;
    }

    const messages = Object.values(errorBody['errors'])
      .flatMap(value => {
        if (Array.isArray(value)) {
          return value.filter((message): message is string => typeof message === 'string');
        }

        return typeof value === 'string' ? [value] : [];
      })
      .filter(message => message.trim());

    return messages.length > 0 ? messages.join(' ') : null;
  }

  private isRecord(value: unknown): value is Record<string, unknown> {
    return typeof value === 'object' && value !== null;
  }
}
