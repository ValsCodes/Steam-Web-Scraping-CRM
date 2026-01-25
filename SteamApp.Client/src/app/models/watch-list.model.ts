export interface WatchList {
  id: number;
  gameId: number;
  gameName: string;
  gameUrlId: number;
  gameUrlName: string;
  productId?: number | null;
  productName?: string | null;
  fullUrl?: string | null;
  rating?: number | null;
  batchNumber?: number | null;
  name?: string | null;
  releaseDate: string; // ISO string from API
  description?: string | null;
  isActive: boolean;
}

export interface CreateWatchList {
  gameUrlId?: number | null;
  productId?: number | null;
  rating?: number | null;
  batchNumber?: number | null;
  name?: string | null;
  releaseDate: string;
  description?: string | null;
  isActive?: boolean | null;
}

export interface UpdateWatchList {
  gameUrlId?: number | null;
  productId?: number | null;
  rating?: number | null;
  batchNumber?: number | null;
  name?: string | null;
  releaseDate: string;
  description?: string | null;
  isActive: boolean;
}
