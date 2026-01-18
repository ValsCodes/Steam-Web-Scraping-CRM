export interface WishList {
  id: number;
  gameId: number;
  price?: number | null;
  isActive: boolean;
}

export interface CreateWishList {
  gameId: number;
  price?: number | null;
  isActive: boolean;
}

export interface UpdateWishList {
  price?: number | null;
  isActive: boolean;
}
