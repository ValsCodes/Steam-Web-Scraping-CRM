export interface WatchList {
  id: number;
  gameId?: number | null;
  gameName: string;
  gameUrlId: number;
  gameUrlName: string;
  productId?: number | null;
  productName?: string | null;
  rating?: number | null;
  batchNumber?: number | null;
  name?: string | null;
  releaseDate: string; // ISO string from API
  description?: string | null;
}

export interface CreateWatchList {
  gameUrlId?: number | null;
  productId?: number | null;
  rating?: number | null;
  batchNumber?: number | null;
  name?: string | null;
  releaseDate: string;
  description?: string | null;
}

export interface UpdateWatchList {
  gameUrlId?: number | null;
  productId?: number | null;
  rating?: number | null;
  batchNumber?: number | null;
  name?: string | null;
  releaseDate: string;
  description?: string | null;
}
