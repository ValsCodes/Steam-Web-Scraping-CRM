import { CommonModule } from '@angular/common';
import { ChangeDetectionStrategy, Component, OnInit, inject, signal } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { finalize, forkJoin } from 'rxjs';

import { AdminAutomationUsage, AutomationPolicy } from '../../../models';
import { AutomationService } from '../../../services';

@Component({
  selector: 'steam-admin-automation-page',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './admin-automation-page.html',
  styleUrl: './admin-automation-page.scss',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class AdminAutomationPage implements OnInit {
  private readonly automationService = inject(AutomationService);

  readonly policy = signal<AutomationPolicy | null>(null);
  readonly usage = signal<AdminAutomationUsage[]>([]);
  readonly busy = signal(false);
  readonly errorMessage = signal('');
  readonly successMessage = signal('');
  limitMinutes = 15;

  ngOnInit(): void {
    this.load();
  }

  load(): void {
    this.busy.set(true);
    this.errorMessage.set('');
    forkJoin({
      policy: this.automationService.getPolicy(),
      usage: this.automationService.getAdminUsage(),
    }).pipe(finalize(() => this.busy.set(false))).subscribe({
      next: ({ policy, usage }) => {
        this.policy.set(policy);
        this.limitMinutes = policy.nonAdminLimitMinutes;
        this.usage.set(usage);
      },
      error: () => this.errorMessage.set('Unable to load automation administration data.'),
    });
  }

  savePolicy(): void {
    const policy = this.policy();
    if (!policy || this.busy() || !Number.isInteger(this.limitMinutes) || this.limitMinutes < 0) return;
    this.busy.set(true);
    this.errorMessage.set('');
    this.automationService.updatePolicy({
      nonAdminLimitMinutes: this.limitMinutes,
      rowVersion: policy.rowVersion,
    }).pipe(finalize(() => this.busy.set(false))).subscribe({
      next: (updated) => {
        this.policy.set(updated);
        this.successMessage.set('Automation policy updated.');
      },
      error: () => this.errorMessage.set('Unable to update the policy. Refresh if another admin changed it.'),
    });
  }

  resetUsage(): void {
    if (this.busy() || !confirm('Reset the rolling automation usage window for every user?')) return;
    this.busy.set(true);
    this.errorMessage.set('');
    this.automationService.resetUsage().pipe(finalize(() => this.busy.set(false))).subscribe({
      next: (updated) => {
        this.policy.set(updated);
        this.successMessage.set('Automation usage reset for all users.');
        this.load();
      },
      error: () => this.errorMessage.set('Unable to reset automation usage.'),
    });
  }

  formatDuration(seconds: number | null): string {
    if (seconds === null) return 'Unlimited';
    const value = Math.max(0, seconds);
    return `${Math.floor(value / 60)}m ${value % 60}s`;
  }
}
