export interface WatchList {
  id: number;
  gameId?: number | null;
  gameUrlId?: number | null;
  rating?: number | null;
  batchUrl?: string | null;
  name?: string | null;
  releaseDate: string; // ISO string from API
  description?: string | null;
}

export interface CreateWatchList {
  gameId?: number | null;
  gameUrlId?: number | null;
  rating?: number | null;
  batchUrl?: string | null;
  name?: string | null;
  releaseDate: string;
  description?: string | null;
}

export interface UpdateWatchList {
  gameId?: number | null;
  gameUrlId?: number | null;
  rating?: number | null;
  batchUrl?: string | null;
  name?: string | null;
  releaseDate: string;
  description?: string | null;
}
