export interface GameUrl {
  id: number;
  name: string | null;
  gameId: number;
  gameName: string;
  itemGroupId: number | null;
  itemGroupName: string | null;
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
  itemGroupId: number | null;
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
  itemGroupId: number | null;
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
