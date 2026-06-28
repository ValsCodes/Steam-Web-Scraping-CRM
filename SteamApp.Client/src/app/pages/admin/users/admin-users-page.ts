import { CommonModule } from '@angular/common';
import { HttpErrorResponse } from '@angular/common/http';
import { Component, OnInit } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { finalize } from 'rxjs';

import {
  AdminUserEffectiveRole,
  AdminUserService,
  AdminUserSummary,
} from '../../../services';

@Component({
  selector: 'steam-admin-users-page',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './admin-users-page.html',
  styleUrl: './admin-users-page.scss',
})
export class AdminUsersPage implements OnInit {
  readonly roleOptions: AdminUserEffectiveRole[] = ['User', 'Admin'];

  users: AdminUserSummary[] = [];
  isLoading = false;
  errorMessage: string | null = null;
  statusMessage: string | null = null;

  private readonly updatingIds = new Set<string>();

  constructor(private readonly adminUserService: AdminUserService) {}

  ngOnInit(): void {
    this.loadUsers();
  }

  loadUsers(): void {
    this.isLoading = true;
    this.errorMessage = null;

    this.adminUserService
      .getUsers()
      .pipe(finalize(() => {
        this.isLoading = false;
      }))
      .subscribe({
        next: users => {
          this.users = users;
        },
        error: error => {
          this.errorMessage = this.getErrorMessage(error, 'Unable to load users');
        },
      });
  }

  roleChanged(user: AdminUserSummary, role: AdminUserEffectiveRole): void {
    if (user.effectiveRole === role || this.isUpdating(user.id)) {
      return;
    }

    this.errorMessage = null;
    this.statusMessage = null;
    this.updatingIds.add(user.id);

    this.adminUserService
      .updateRole(user.id, role)
      .pipe(finalize(() => {
        this.updatingIds.delete(user.id);
      }))
      .subscribe({
        next: updated => {
          this.users = this.users.map(item =>
            item.id === updated.id ? updated : item,
          );
          this.statusMessage = `${updated.displayName} is now ${updated.effectiveRole}`;
        },
        error: error => {
          this.errorMessage = this.getErrorMessage(error, 'Unable to update role');
        },
      });
  }

  isUpdating(id: string): boolean {
    return this.updatingIds.has(id);
  }

  canChangeRole(user: AdminUserSummary): boolean {
    return !(user.isCurrentUser && user.effectiveRole === 'Admin');
  }

  trackByUserId(_index: number, user: AdminUserSummary): string {
    return user.id;
  }

  private getErrorMessage(error: unknown, fallback: string): string {
    if (!(error instanceof HttpErrorResponse)) {
      return fallback;
    }

    if (this.isRecord(error.error) && typeof error.error['message'] === 'string') {
      return error.error['message'];
    }

    if (typeof error.error === 'string' && error.error.trim()) {
      return error.error;
    }

    return fallback;
  }

  private isRecord(value: unknown): value is Record<string, unknown> {
    return typeof value === 'object' && value !== null;
  }
}
