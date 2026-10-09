import { ComponentFixture, TestBed } from '@angular/core/testing';
import { of } from 'rxjs';

import { AdminAutomationUsage, AutomationPolicy } from '../../../models';
import { AutomationService } from '../../../services';
import { AdminAutomationPage } from './admin-automation-page';

describe('AdminAutomationPage', () => {
  let fixture: ComponentFixture<AdminAutomationPage>;
  let component: AdminAutomationPage;
  let automationService: jasmine.SpyObj<AutomationService>;

  const policy: AutomationPolicy = {
    nonAdminLimitMinutes: 15,
    usageResetAtUtc: null,
    lastModifiedByUserId: null,
    lastModifiedAtUtc: '2026-10-07T14:00:00Z',
    lastResetByUserId: null,
    lastResetAtUtc: null,
    rowVersion: 'AQID',
  };
  const usage: AdminAutomationUsage[] = [{
    userId: 'user-1',
    email: 'user@example.test',
    isAdmin: false,
    usage: {
      unlimited: false,
      limitSeconds: 900,
      usedSeconds: 120,
      remainingSeconds: 780,
      usageResetAtUtc: null,
      presenceRequired: true,
      presenceActive: true,
      presenceExpiresAtUtc: '2026-10-07T14:00:30Z',
      nextAllowanceAtUtc: null,
    },
  }];

  beforeEach(async () => {
    automationService = jasmine.createSpyObj<AutomationService>('AutomationService', [
      'getPolicy',
      'getAdminUsage',
      'updatePolicy',
      'resetUsage',
    ]);
    automationService.getPolicy.and.returnValue(of(policy));
    automationService.getAdminUsage.and.returnValue(of(usage));

    await TestBed.configureTestingModule({
      imports: [AdminAutomationPage],
      providers: [{ provide: AutomationService, useValue: automationService }],
    }).compileComponents();

    fixture = TestBed.createComponent(AdminAutomationPage);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('loads policy and per-user usage', () => {
    expect(component.policy()).toEqual(policy);
    expect(component.usage()).toEqual(usage);
    expect(component.limitMinutes).toBe(15);
    expect(component.formatDuration(780)).toBe('13m 0s');
  });

  it('allows an administrator to set a zero-minute limit', () => {
    const updated = { ...policy, nonAdminLimitMinutes: 0, rowVersion: 'BAUG' };
    automationService.updatePolicy.and.returnValue(of(updated));
    component.limitMinutes = 0;

    component.savePolicy();

    expect(automationService.updatePolicy).toHaveBeenCalledWith({
      nonAdminLimitMinutes: 0,
      rowVersion: policy.rowVersion,
    });
    expect(component.policy()).toEqual(updated);
  });

  it('resets all usage after confirmation and reloads the view', () => {
    spyOn(window, 'confirm').and.returnValue(true);
    automationService.resetUsage.and.returnValue(of({
      ...policy,
      usageResetAtUtc: '2026-10-07T14:05:00Z',
    }));

    component.resetUsage();

    expect(automationService.resetUsage).toHaveBeenCalled();
    expect(automationService.getPolicy).toHaveBeenCalledTimes(2);
    expect(component.successMessage()).toContain('reset');
  });
});
