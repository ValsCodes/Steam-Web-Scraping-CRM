export interface GameUrl {
  id: number;
  name?: string | null;
  gameId: number;
  gameName: string;
  partialUrl?: string | null;
  isBatchUrl: boolean;
  startPage?: number | null;
  endPage?: number | null;
  isPixelScrape: boolean;
  pixelX?: number | null;
  pixelY?: number | null;
  pixelImageWidth?: number | null;
  pixelImageHeight?: number | null;
  isPublicApi: boolean;
}

export interface CreateGameUrl {
  gameId: number;
  name?: string | null;
  partialUrl?: string | null;
  isBatchUrl: boolean;
  startPage?: number | null;
  endPage?: number | null;
  isPixelScrape: boolean;
  pixelX?: number | null;
  pixelY?: number | null;
  pixelImageWidth?: number | null;
  pixelImageHeight?: number | null;
  isPublicApi: boolean;

}

export interface UpdateGameUrl {
  partialUrl?: string | null;
  name?: string | null;
  isBatchUrl: boolean;
  startPage?: number | null;
  endPage?: number | null;
  isPixelScrape: boolean;
  pixelX?: number | null;
  pixelY?: number | null;
  pixelImageWidth?: number | null;
  pixelImageHeight?: number | null;
  isPublicApi: boolean;
}
