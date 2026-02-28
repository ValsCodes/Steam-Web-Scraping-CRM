export interface GameUrlProduct {
  productId: number;
  productName: string;
  gameUrlId: number;
  gameUrlName: string;
  isBatchUrl: boolean;
  fullUrl: string;
  tags: string[];
  isActive: boolean;
  rating: number | null;
}

export interface CreateGameUrlProduct {
  productId: number;
  gameUrlId: number;
}