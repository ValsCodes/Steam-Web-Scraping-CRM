import { HttpClient } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { catchError } from 'rxjs/operators';

import { handleError } from '../error-handler';
import * as g from '../general-data';

export type AdminUserEffectiveRole = 'Admin' | 'User';

export interface AdminUserSummary {
  id: string;
  displayName: string;
  firstName: string | null;
  lastName: string | null;
  userName: string | null;
  email: string | null;
  phone: string | null;
  roles: string[];
  effectiveRole: AdminUserEffectiveRole;
  isCurrentUser: boolean;
}

export interface UpdateAdminUserRoleRequest {
  role: AdminUserEffectiveRole;
}

@Injectable({ providedIn: 'root' })
export class AdminUserService {
  private readonly baseUrl = `${g.localHost}api/admin/users`;

  constructor(private readonly http: HttpClient) {}

  getUsers(): Observable<AdminUserSummary[]> {
    return this.http
      .get<AdminUserSummary[]>(this.baseUrl)
      .pipe(catchError(handleError));
  }

  updateRole(
    id: string,
    role: AdminUserEffectiveRole,
  ): Observable<AdminUserSummary> {
    return this.http
      .put<AdminUserSummary>(`${this.baseUrl}/${encodeURIComponent(id)}/role`, { role })
      .pipe(catchError(handleError));
  }
}
