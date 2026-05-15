export interface GameUrl {
  id: number;
  name?: string | null;
  gameId: number;
  gameName: string;
  scrapingModeId?: number | null;
  scrapingModeName?: string | null;
  partialUrl?: string | null;
  startPage?: number | null;
  endPage?: number | null;
  pixelX?: number | null;
  pixelY?: number | null;
  pixelImageWidth?: number | null;
  pixelImageHeight?: number | null;
  isActive: boolean;
}

export interface CreateGameUrl {
  gameId: number;
  name?: string | null;
  scrapingModeId: number;
  partialUrl?: string | null;
  startPage?: number | null;
  endPage?: number | null;
  pixelX?: number | null;
  pixelY?: number | null;
  pixelImageWidth?: number | null;
  pixelImageHeight?: number | null;
  isActive: boolean;
}

export interface UpdateGameUrl {
  partialUrl?: string | null;
  name?: string | null;
  scrapingModeId: number;
  startPage?: number | null;
  endPage?: number | null;
  pixelX?: number | null;
  pixelY?: number | null;
  pixelImageWidth?: number | null;
  pixelImageHeight?: number | null;
  isActive?: boolean | null;
}

export interface UpdateGameUrlStatus {
  id: number;
  isActive: boolean;
}
