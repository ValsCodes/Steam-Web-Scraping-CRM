export interface Product {
  id: number;
  gameId: number;
  gameName: string;
  name?: string | null;
}

export interface CreateProduct {
  gameId: number;
  name?: string | null;
}

export interface UpdateProduct {
  name?: string | null;
}
