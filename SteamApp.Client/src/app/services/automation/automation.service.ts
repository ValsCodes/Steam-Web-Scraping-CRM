import { HttpClient } from '@angular/common/http';
import { Injectable, OnDestroy, signal } from '@angular/core';
import { EMPTY, Subscription, catchError, finalize } from 'rxjs';

import {
  AdminAutomationUsage,
  AutomationPolicy,
  AutomationPolicyUpdate,
  AutomationUsage,
} from '../../models';
import { AuthService } from '../auth/auth.service';
import * as g from '../general-data';

@Injectable({ providedIn: 'root' })
export class AutomationService implements OnDestroy {
  private static readonly tabIdKey = 'steamapp_automation_tab_id';
  private static readonly heartbeatIntervalMs = 10_000;

  private readonly baseUrl = `${g.localHost}api`;
  private readonly tabId = this.getOrCreateTabId();
  private readonly subscriptions = new Subscription();
  private heartbeatTimer: number | null = null;
  private heartbeatPending = false;

  readonly usage = signal<AutomationUsage | null>(null);
  readonly usageLoading = signal(false);

  constructor(
    private readonly http: HttpClient,
    private readonly authService: AuthService,
  ) {
    document.addEventListener('visibilitychange', this.handleVisibilityChange);
    window.addEventListener('pagehide', this.handleSessionEnding);
    window.addEventListener('steamapp:session-ending', this.handleSessionEnding);

    this.subscriptions.add(this.authService.loggedIn$.subscribe((loggedIn) => {
      if (loggedIn) {
        this.startHeartbeat();
      } else {
        this.stopHeartbeat();
        this.usage.set(null);
      }
    }));
  }

  ngOnDestroy(): void {
    this.releasePresence();
    this.stopHeartbeat();
    this.subscriptions.unsubscribe();
    document.removeEventListener('visibilitychange', this.handleVisibilityChange);
    window.removeEventListener('pagehide', this.handleSessionEnding);
    window.removeEventListener('steamapp:session-ending', this.handleSessionEnding);
  }

  refreshUsage(): void {
    if (!this.canReportPresence()) {
      this.usage.set(null);
      return;
    }

    this.usageLoading.set(true);
    this.http.get<AutomationUsage>(`${this.baseUrl}/automation-usage/me`).pipe(
      catchError(() => EMPTY),
      finalize(() => this.usageLoading.set(false)),
    ).subscribe((usage) => this.usage.set(usage));
  }

  getPolicy() {
    return this.http.get<AutomationPolicy>(`${this.baseUrl}/admin/automation-policy`);
  }

  updatePolicy(input: AutomationPolicyUpdate) {
    return this.http.put<AutomationPolicy>(`${this.baseUrl}/admin/automation-policy`, input);
  }

  resetUsage() {
    return this.http.post<AutomationPolicy>(`${this.baseUrl}/admin/automation-policy/reset-usage`, {});
  }

  getAdminUsage() {
    return this.http.get<AdminAutomationUsage[]>(`${this.baseUrl}/admin/automation-usage`);
  }

  private startHeartbeat(): void {
    if (this.heartbeatTimer !== null) {
      return;
    }

    this.heartbeat();
    this.heartbeatTimer = window.setInterval(
      () => this.heartbeat(),
      AutomationService.heartbeatIntervalMs,
    );
  }

  private stopHeartbeat(): void {
    if (this.heartbeatTimer !== null) {
      window.clearInterval(this.heartbeatTimer);
      this.heartbeatTimer = null;
    }
  }

  private heartbeat(): void {
    if (!this.canReportPresence() || this.heartbeatPending) {
      return;
    }

    this.heartbeatPending = true;
    this.http.put<void>(`${this.baseUrl}/session-presence/${this.tabId}`, {}).pipe(
      catchError(() => EMPTY),
      finalize(() => this.heartbeatPending = false),
    ).subscribe(() => this.refreshUsage());
  }

  private readonly handleVisibilityChange = (): void => {
    if (document.visibilityState === 'visible') {
      this.startHeartbeat();
      this.heartbeat();
      return;
    }

    this.releasePresence();
  };

  private readonly handleSessionEnding = (): void => {
    this.releasePresence();
  };

  private releasePresence(): void {
    const token = this.authService.getToken();
    if (!token) {
      return;
    }

    void fetch(`${this.baseUrl}/session-presence/${this.tabId}`, {
      method: 'DELETE',
      headers: { Authorization: `Bearer ${token}` },
      keepalive: true,
      credentials: 'same-origin',
    }).catch(() => undefined);
  }

  private canReportPresence(): boolean {
    return document.visibilityState === 'visible' && this.authService.isLoggedIn();
  }

  private getOrCreateTabId(): string {
    const existing = sessionStorage.getItem(AutomationService.tabIdKey);
    if (existing) {
      return existing;
    }

    const tabId = crypto.randomUUID();
    sessionStorage.setItem(AutomationService.tabIdKey, tabId);
    return tabId;
  }
}
