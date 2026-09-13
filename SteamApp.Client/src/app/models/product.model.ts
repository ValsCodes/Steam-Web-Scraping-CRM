export interface Product {
  id: number;
  gameId: number;
  gameName: string;
  fullUrl?: string | null;
  name?: string | null;
  isActive: boolean;
  tags: string[];
  tagDetails?: ProductTagDetail[];
  rating?: number | null;
  gameInternalId?: number | null;
}

export interface ProductTagDetail {
  id: number;
  name: string | null;
  isActive: boolean;
  itemGroupId: number | null;
  itemGroupName: string | null;
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

export interface UpdateProductStatus {
  id: number;
  isActive: boolean;
}
