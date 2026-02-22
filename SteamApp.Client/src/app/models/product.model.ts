export interface Product {
  id: number;
  gameId: number;
  gameName: string;
  fullUrl?: string | null;
  name?: string | null;
  isActive: boolean;
  tags: string[];
  rating?: number | null;
}

export interface CreateProduct {
  gameId: number;
  name?: string | null;
  isActive?: boolean | null;
  rating?: number | null;
}

export interface UpdateProduct {
  name?: string | null;
  isActive?: boolean | null;
  rating?: number | null;
}
