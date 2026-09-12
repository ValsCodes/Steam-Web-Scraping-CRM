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
  currentStock: number;
}

export interface CreateGameUrlProduct {
  productId: number;
  gameUrlId: number;
}

export interface GameUrlProductCurrentStock {
  currentStock: number;
}

export enum GameUrlProductStockOperation {
  Assigned = 1,
  Incremented = 2,
  Decremented = 3,
}

export interface GameUrlProductStockHistory {
  id: number;
  productId: number;
  gameUrlId: number;
  previousStock: number;
  newStock: number;
  operation: GameUrlProductStockOperation;
  createdAtUtc: string;
}

export interface GameUrlProductStockHistoryPage {
  items: GameUrlProductStockHistory[];
  pageNumber: number;
  pageSize: number;
  totalCount: number;
  totalPages: number;
}
