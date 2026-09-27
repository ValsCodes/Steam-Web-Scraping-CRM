import { ManualCheckCriterion } from './manual-check.model';

export type AutomaticQueueBlockType = 'ManualCheck' | 'Delay';
export type AutomaticQueueTemplateMode = 'SavedPreset' | 'PrivateTemplate';
export type AutomaticQueueRunStatus =
  | 'Queued' | 'Running' | 'PauseRequested' | 'Paused'
  | 'Succeeded' | 'CompletedWithErrors' | 'Failed' | 'Canceled';
export type AutomaticQueueBlockRunStatus =
  | 'Pending' | 'Running' | 'Paused' | 'Succeeded'
  | 'CompletedWithWarnings' | 'Failed' | 'Canceled' | 'Skipped';

export interface AutomaticQueuePrivateTemplate {
  name: string;
  listingLimit: number;
  cooldownMinutes: number | null;
  cooldownSeconds: number | null;
  criteria: ManualCheckCriterion[];
}

export interface AutomaticQueueBlockWrite {
  key: string;
  type: AutomaticQueueBlockType;
  delaySeconds: number | null;
  gameUrlId: number | null;
  templateMode: AutomaticQueueTemplateMode | null;
  presetId: number | null;
  privateTemplate: AutomaticQueuePrivateTemplate | null;
  bypassCache: boolean;
  productIds: number[] | null;
}

export interface AutomaticQueueBlockDraft extends AutomaticQueueBlockWrite {
  gameId: number | null;
  gameName: string | null;
  gameUrlName: string | null;
  presetName: string | null;
}

export interface AutomaticQueueBlock extends AutomaticQueueBlockDraft {
  id: number;
  sortOrder: number;
}

export interface AutomaticQueueWrite {
  name: string;
  blocks: AutomaticQueueBlockWrite[];
}

export interface AutomaticQueueDefinition {
  id: number;
  name: string;
  blocks: AutomaticQueueBlock[];
  createdAtUtc: string;
  updatedAtUtc: string;
  activeRunId: number | null;
}

export interface AutomaticQueueRunBlock {
  id: number;
  key: string;
  sortOrder: number;
  type: AutomaticQueueBlockType;
  status: AutomaticQueueBlockRunStatus;
  configuration: AutomaticQueueBlock;
  manualCheckRunId: number | null;
  warningCount: number;
  waitUntilUtc: string | null;
  remainingDelaySeconds: number | null;
  startedAtUtc: string | null;
  completedAtUtc: string | null;
  errorText: string | null;
}

export interface AutomaticQueueRun {
  id: number;
  queueId: number | null;
  queueName: string;
  status: AutomaticQueueRunStatus;
  currentBlockIndex: number;
  totalBlocks: number;
  completedBlocks: number;
  date: string;
  startedAtUtc: string | null;
  completedAtUtc: string | null;
  errorText: string | null;
  correlationId: string;
  blocks: AutomaticQueueRunBlock[];
}

export interface AutomaticQueueRunAccepted {
  runId: number;
  run: AutomaticQueueRun;
}
