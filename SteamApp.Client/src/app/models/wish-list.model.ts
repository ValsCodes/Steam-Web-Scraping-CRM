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
