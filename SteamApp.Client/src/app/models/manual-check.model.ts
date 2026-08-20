export type ManualCheckRunStatus =
  | 'Queued'
  | 'Running'
  | 'Succeeded'
  | 'CompletedWithErrors'
  | 'Failed'
  | 'Canceled';

export interface ManualCheckCriterion {
  conditionOperatorId: number | null;
  conditionOperatorName?: string | null;
  nameContains: string | null;
  valueContains: string | null;
}

export interface ManualCheckConditionOperator {
  id: number;
  name: string;
}

export interface ManualCheckPresetWrite {
  gameId: number;
  name: string;
  listingLimit: number;
  cooldownMinutes: number | null;
  cooldownSeconds: number | null;
  criteria: ManualCheckCriterion[];
}

export interface ManualCheckPreset extends ManualCheckPresetWrite {
  id: number;
  gameName: string | null;
  createdAtUtc: string;
  updatedAtUtc: string;
}

export interface ManualCheckRunRequest {
  gameUrlId: number;
  presetId: number;
  bypassCache: boolean;
}

export interface ManualCheckProgress {
  totalProducts: number;
  checkedProducts: number;
  matchedProducts: number;
  failedProducts: number;
}

export interface ManualCheckRunSummary {
  id: number;
  presetId: number | null;
  presetName: string;
  gameId: number;
  gameName: string | null;
  gameUrlId: number;
  gameUrlName: string | null;
  totalProducts: number;
  checkedProducts: number;
  matchedProducts: number;
  failedProducts: number;
  progress: ManualCheckProgress;
  status: ManualCheckRunStatus;
  date: string;
  startedAtUtc: string | null;
  completedAtUtc: string | null;
  durationMilliseconds: number | null;
  correlationId: string;
  errorText: string | null;
}

export interface ManualCheckRunAccepted {
  runId: number;
  run: ManualCheckRunSummary;
}

export interface ManualCheckProductInput {
  productId: number;
  productName: string;
  gameUrlId: number;
  gameUrlName: string;
  fullUrl: string;
  tags: string[];
  rating: number | null;
}

export interface ManualCheckSetup {
  presetId: number | null;
  presetName: string;
  gameId: number;
  gameName: string | null;
  gameUrlId: number;
  gameUrlName: string | null;
  listingLimit: number;
  cooldownMinutes: number | null;
  cooldownSeconds: number | null;
  bypassCache: boolean;
  criteria: ManualCheckCriterion[];
  products: ManualCheckProductInput[];
  requestedAtUtc: string;
}

export interface ManualCheckDescriptionMatch {
  name: string;
  value: string;
  color: string;
  matchedCriterionIndexes: number[];
}

export interface ManualCheckAssetMatch {
  appId: string;
  contextId: string;
  assetId: string;
  classId: string;
  instanceId: string;
  marketName: string;
  iconUrl: string;
  descriptions: ManualCheckDescriptionMatch[];
}

export interface ManualCheckProductResult extends ManualCheckProductInput {
  matchedAssets: ManualCheckAssetMatch[];
}

export interface ManualCheckProductTrace {
  productId: number;
  productName: string;
  fullUrl: string;
  matchEvaluated: boolean;
  matched: boolean;
  matchedAssetCount: number;
  steamApiResultJson: string | null;
  durationMilliseconds: number | null;
}

export interface ManualCheckProductError {
  productId: number;
  productName: string;
  fullUrl: string;
  error: string;
  errorType?: string | null;
  httpStatusCode?: number | null;
  occurredAtUtc?: string | null;
}

export interface ManualCheckRunResults {
  matches: ManualCheckProductResult[];
  productTraces: ManualCheckProductTrace[];
  errors: ManualCheckProductError[];
}

export interface ManualCheckRunDetail extends ManualCheckRunSummary {
  setup: ManualCheckSetup;
  results: ManualCheckRunResults;
}
