export enum FeedbackRequestType {
  Feedback = 1,
  Bug = 2,
}

export enum FeedbackRequestStatus {
  Active = 1,
  Processed = 2,
  Closed = 3,
}

export enum FeedbackRequestHistoryAction {
  Created = 1,
  Updated = 2,
  StatusChanged = 3,
}

export interface FeedbackRequest {
  id: number;
  referenceId: string;
  type: FeedbackRequestType;
  title: string;
  description: string;
  area?: string | null;
  status: FeedbackRequestStatus;
  createdAtUtc: string;
  updatedAtUtc: string;
  statusChangedAtUtc: string;
}

export interface CreateFeedbackRequest {
  type: FeedbackRequestType;
  title: string;
  description: string;
  area?: string | null;
}

export interface UpdateFeedbackRequest {
  type: FeedbackRequestType;
  title: string;
  description: string;
  area?: string | null;
  status: FeedbackRequestStatus;
}

export interface UpdateFeedbackRequestStatus {
  status: FeedbackRequestStatus;
}

export interface FeedbackRequestHistory {
  id: number;
  feedbackRequestId: number;
  action: FeedbackRequestHistoryAction;
  createdAtUtc: string;
  previousType?: FeedbackRequestType | null;
  newType?: FeedbackRequestType | null;
  previousTitle?: string | null;
  newTitle?: string | null;
  previousDescription?: string | null;
  newDescription?: string | null;
  previousArea?: string | null;
  newArea?: string | null;
  previousStatus?: FeedbackRequestStatus | null;
  newStatus?: FeedbackRequestStatus | null;
}

export interface FeedbackRequestOption<TValue extends number> {
  value: TValue;
  label: string;
}

export const FEEDBACK_REQUEST_TYPE_OPTIONS: readonly FeedbackRequestOption<FeedbackRequestType>[] = [
  { value: FeedbackRequestType.Feedback, label: 'Feedback' },
  { value: FeedbackRequestType.Bug, label: 'Bug' },
];

export const FEEDBACK_REQUEST_STATUS_OPTIONS: readonly FeedbackRequestOption<FeedbackRequestStatus>[] = [
  { value: FeedbackRequestStatus.Active, label: 'Active' },
  { value: FeedbackRequestStatus.Processed, label: 'Processed' },
  { value: FeedbackRequestStatus.Closed, label: 'Closed' },
];

export const FEEDBACK_REQUEST_HISTORY_ACTION_OPTIONS: readonly FeedbackRequestOption<FeedbackRequestHistoryAction>[] = [
  { value: FeedbackRequestHistoryAction.Created, label: 'Created' },
  { value: FeedbackRequestHistoryAction.Updated, label: 'Updated' },
  { value: FeedbackRequestHistoryAction.StatusChanged, label: 'Status Changed' },
];

export function feedbackRequestTypeLabel(value: FeedbackRequestType): string {
  return FEEDBACK_REQUEST_TYPE_OPTIONS.find((option) => option.value === value)?.label ?? 'Unknown';
}

export function feedbackRequestStatusLabel(value: FeedbackRequestStatus): string {
  return FEEDBACK_REQUEST_STATUS_OPTIONS.find((option) => option.value === value)?.label ?? 'Unknown';
}

export function feedbackRequestHistoryActionLabel(value: FeedbackRequestHistoryAction): string {
  return FEEDBACK_REQUEST_HISTORY_ACTION_OPTIONS.find((option) => option.value === value)?.label ?? 'Unknown';
}
