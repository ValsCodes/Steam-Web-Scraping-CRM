export interface GameUrlProduct {
  productId: number;
  productName: string;
  gameUrlId: number;
  gameUrlName: string;
  scrapingModeId?: number | null;
  scrapingModeName?: string | null;
  fullUrl: string;
  tags: string[];
  isActive: boolean;
  rating: number | null;
}

export interface CreateGameUrlProduct {
  productId: number;
  gameUrlId: number;
}
