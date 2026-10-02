export interface WishList {
  id: number;
  gameId: number;
  gameName: string;
  name: string;
  pageUrl: string;
  price?: number | null;
  isActive: boolean;
}

export interface CreateWishList {
  gameId: number;
  name?: string | null;
  price?: number | null;
  isActive: boolean;
}

export interface UpdateWishList {
  price?: number | null;
  name?: string | null;
  isActive: boolean;
}

export interface UpdateWishListStatus {
  id: number;
  isActive: boolean;
}

export type WishListCheckSource = 'Manual' | 'Scheduled';
export type WishListCheckStatus = 'Running' | 'Succeeded' | 'Failed' | 'Canceled';

export interface WishListCheckHistory {
  id: number;
  wishListId: number;
  gameName: string;
  source: WishListCheckSource;
  status: WishListCheckStatus;
  targetPrice: number | null;
  currentPrice: number | null;
  isPriceReached: boolean | null;
  requestedAtUtc: string;
  startedAtUtc: string;
  completedAtUtc: string | null;
  durationMilliseconds: number | null;
  correlationId: string;
  errorCode: string | null;
  errorText: string | null;
}

export interface WishListCheckHistoryPage {
  items: WishListCheckHistory[];
  pageNumber: number;
  pageSize: number;
  totalCount: number;
  totalPages: number;
}
