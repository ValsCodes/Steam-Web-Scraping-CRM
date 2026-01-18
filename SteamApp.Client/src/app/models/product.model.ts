export interface Product {
  id: number;
  gameUrlId: number;
  name?: string | null;
}

export interface CreateProduct {
  gameUrlId: number;
  name?: string | null;
}

export interface UpdateProduct {
  name?: string | null;
}
