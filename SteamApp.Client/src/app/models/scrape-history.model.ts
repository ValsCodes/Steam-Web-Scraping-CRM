import { Listing } from './listing.model';

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
