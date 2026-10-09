export type AutomationPauseReason =
  | 'UserRequested'
  | 'PresenceLost'
  | 'QuotaExceeded'
  | 'SystemRecovery';

export interface AutomationUsage {
  unlimited: boolean;
  limitSeconds: number | null;
  usedSeconds: number;
  remainingSeconds: number | null;
  usageResetAtUtc: string | null;
  presenceRequired: boolean;
  presenceActive: boolean;
  presenceExpiresAtUtc: string | null;
  nextAllowanceAtUtc: string | null;
}

export interface AutomationPolicy {
  nonAdminLimitMinutes: number;
  usageResetAtUtc: string | null;
  lastModifiedByUserId: string | null;
  lastModifiedAtUtc: string;
  lastResetByUserId: string | null;
  lastResetAtUtc: string | null;
  rowVersion: string;
}

export interface AutomationPolicyUpdate {
  nonAdminLimitMinutes: number;
  rowVersion: string;
}

export interface AdminAutomationUsage {
  userId: string;
  email: string | null;
  isAdmin: boolean;
  usage: AutomationUsage;
}
