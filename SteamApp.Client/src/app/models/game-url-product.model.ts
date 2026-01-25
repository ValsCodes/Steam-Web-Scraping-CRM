export interface GameUrlProduct {
  productId: number;
  productName: string;
  gameUrlId: number;
  gameUrlName: string;
  isBatchUrl: boolean;
  fullUrl: string;
}

export interface CreateGameUrlProduct {
  productId: number;
  gameUrlId: number;
}