export interface GameUrl {
  id: number;
  gameId: number;
  partialUrl?: string | null;
  isBatchUrl: boolean;
  startPage?: number | null;
  endPage?: number | null;
  isPixelScrape: boolean;
}

export interface CreateGameUrl {
  gameId: number;
  partialUrl?: string | null;
  isBatchUrl: boolean;
  startPage?: number | null;
  endPage?: number | null;
  isPixelScrape: boolean;
}

export interface UpdateGameUrl {
  partialUrl?: string | null;
  isBatchUrl: boolean;
  startPage?: number | null;
  endPage?: number | null;
  isPixelScrape: boolean;
}
