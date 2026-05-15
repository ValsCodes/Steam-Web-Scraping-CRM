import { CommonModule } from '@angular/common';
import { HttpErrorResponse } from '@angular/common/http';
import { Component, DestroyRef, OnInit, inject } from '@angular/core';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { MatDialog } from '@angular/material/dialog';
import { Router } from '@angular/router';
import { finalize } from 'rxjs';

import { ConfirmDialogComponent } from '../../components/confirm-dialog.component';
import {
  AuthService,
  UpdateUserProfileRequest,
  UserProfile,
} from '../../services';

interface PasswordRequirement {
  label: string;
  isMet: boolean;
}

@Component({
  selector: 'steam-profile-page',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule],
  templateUrl: './profile-page.html',
  styleUrl: './profile-page.scss',
})
export class ProfilePage implements OnInit {
  private readonly destroyRef = inject(DestroyRef);

  isLoading = false;
  isSavingProfile = false;
  isChangingPassword = false;
  isDeleting = false;

  profileError: string | null = null;
  profileMessage: string | null = null;
  passwordError: string | null = null;
  passwordMessage: string | null = null;
  deleteError: string | null = null;

  private profile: UserProfile | null = null;

  readonly profileForm = this.fb.nonNullable.group({
    firstName: ['', [Validators.maxLength(100)]],
    lastName: ['', [Validators.maxLength(100)]],
    userName: [''],
    email: ['', [Validators.required, Validators.email]],
    phone: [''],
  });

  readonly passwordForm = this.fb.nonNullable.group({
    currentPassword: [''],
    newPassword: [''],
    confirmPassword: [''],
  });

  readonly deleteForm = this.fb.nonNullable.group({
    password: [''],
  });

  constructor(
    private readonly auth: AuthService,
    private readonly fb: FormBuilder,
    private readonly dialog: MatDialog,
    private readonly router: Router,
  ) {}

  ngOnInit(): void {
    this.loadProfile();
  }

  get passwordRequirements(): PasswordRequirement[] {
    const password = this.passwordForm.controls.newPassword.value;

    return [
      { label: 'At least 8 characters', isMet: password.length >= 8 },
      { label: 'One uppercase letter', isMet: /[A-Z]/.test(password) },
      { label: 'One lowercase letter', isMet: /[a-z]/.test(password) },
      { label: 'One number', isMet: /\d/.test(password) },
    ];
  }

  saveProfile(): void {
    if (this.profileForm.invalid || this.isSavingProfile) {
      this.profileForm.markAllAsTouched();
      return;
    }

    this.profileError = null;
    this.profileMessage = null;
    this.isSavingProfile = true;

    this.auth
      .updateProfile(this.createProfileRequest())
      .pipe(
        takeUntilDestroyed(this.destroyRef),
        finalize(() => {
          this.isSavingProfile = false;
        }),
      )
      .subscribe({
        next: profile => {
          this.profile = profile;
          this.profileMessage = 'Profile saved';
          this.profileForm.markAsPristine();
        },
        error: error => {
          this.profileError = this.getErrorMessage(error, 'Profile update failed');
        },
      });
  }

  changePassword(): void {
    if (this.isChangingPassword) {
      return;
    }

    this.passwordError = null;
    this.passwordMessage = null;

    const currentPassword = this.passwordForm.controls.currentPassword.value;
    const newPassword = this.passwordForm.controls.newPassword.value;
    const confirmPassword = this.passwordForm.controls.confirmPassword.value;

    if (!currentPassword || !newPassword) {
      this.passwordError = 'Enter your current password and a new password';
      return;
    }

    const missingRequirements = this.passwordRequirements
      .filter(requirement => !requirement.isMet)
      .map(requirement => requirement.label.toLowerCase());

    if (missingRequirements.length > 0) {
      this.passwordError = `Password must include ${missingRequirements.join(', ')}`;
      return;
    }

    if (newPassword !== confirmPassword) {
      this.passwordError = 'Passwords do not match';
      return;
    }

    this.isChangingPassword = true;

    this.auth
      .changePassword({ currentPassword, newPassword })
      .pipe(
        takeUntilDestroyed(this.destroyRef),
        finalize(() => {
          this.isChangingPassword = false;
        }),
      )
      .subscribe({
        next: () => {
          this.passwordForm.reset();
          this.passwordMessage = 'Password changed';
        },
        error: error => {
          this.passwordError = this.getErrorMessage(error, 'Password change failed');
        },
      });
  }

  deleteAccount(): void {
    if (this.isDeleting) {
      return;
    }

    this.deleteError = null;

    const password = this.deleteForm.controls.password.value;
    if (!password) {
      this.deleteError = 'Enter your password';
      return;
    }

    this.dialog
      .open(ConfirmDialogComponent, {
        width: '420px',
        data: {
          title: 'Delete Account',
          subtitle: 'This action cannot be undone.',
          message: 'Your profile and login will be removed.',
          confirmText: 'Delete account',
          cancelText: 'Cancel',
        },
      })
      .afterClosed()
      .pipe(takeUntilDestroyed(this.destroyRef))
      .subscribe((confirmed: boolean) => {
        if (!confirmed) {
          return;
        }

        this.isDeleting = true;

        this.auth
          .deleteProfile({ password })
          .pipe(
            takeUntilDestroyed(this.destroyRef),
            finalize(() => {
              this.isDeleting = false;
            }),
          )
          .subscribe({
            next: () => {
              this.router.navigate(['/login'], { replaceUrl: true });
            },
            error: error => {
              this.deleteError = this.getErrorMessage(error, 'Account deletion failed');
            },
          });
      });
  }

  resetProfileForm(): void {
    if (!this.profile) {
      return;
    }

    this.patchProfileForm(this.profile);
    this.profileError = null;
    this.profileMessage = null;
  }

  private loadProfile(): void {
    this.isLoading = true;
    this.profileError = null;

    this.auth
      .getProfile()
      .pipe(
        takeUntilDestroyed(this.destroyRef),
        finalize(() => {
          this.isLoading = false;
        }),
      )
      .subscribe({
        next: profile => {
          this.profile = profile;
          this.patchProfileForm(profile);
        },
        error: error => {
          this.profileError = this.getErrorMessage(error, 'Profile load failed');
        },
      });
  }

  private patchProfileForm(profile: UserProfile): void {
    this.profileForm.reset({
      firstName: profile.firstName ?? '',
      lastName: profile.lastName ?? '',
      userName: profile.userName ?? '',
      email: profile.email ?? '',
      phone: profile.phone ?? '',
    });
  }

  private createProfileRequest(): UpdateUserProfileRequest {
    const value = this.profileForm.getRawValue();

    return {
      firstName: this.normalizeOptional(value.firstName),
      lastName: this.normalizeOptional(value.lastName),
      userName: this.normalizeOptional(value.userName),
      email: value.email.trim(),
      phone: this.normalizeOptional(value.phone),
    };
  }

  private normalizeOptional(value: string): string | null {
    const trimmed = value.trim();
    return trimmed ? trimmed : null;
  }

  private getErrorMessage(error: unknown, fallback: string): string {
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
