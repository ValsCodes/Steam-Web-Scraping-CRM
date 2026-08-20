import { Listing } from './listing.model';

export type ScrapeJobStatus = 'Queued' | 'Running' | 'Succeeded' | 'Failed';

export interface ScrapeHistory {
  id: number;
  endpoint: string;
  scrapeType: string;
  gameUrlId: number;
  gameUrlName?: string | null;
  page: number;
  resultCount: number;
  date: string;
  isHaveError: boolean;
  status: ScrapeJobStatus;
  startedAtUtc?: string | null;
  completedAtUtc?: string | null;
  correlationId?: string | null;
}

export interface ScrapeHistoryDetail extends ScrapeHistory {
  setupJson: string;
  resultsJson?: string | null;
  errorText?: string | null;
}

export interface ScrapeHistoryRerunResponse {
  history: ScrapeHistory;
  results: Listing[];
  errorText?: string | null;
}

export interface ScrapeJobAccepted {
  historyId: number;
  history: ScrapeHistory;
  status: ScrapeJobStatus;
  correlationId: string;
}
