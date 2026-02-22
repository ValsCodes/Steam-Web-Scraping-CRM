// src/app/models/product-tag.model.ts

export interface ProductTag {
  productId: number;
  productName?: string;
  gameId: number;
  gameName?: string;
  tagId: number;
  tagName?: string;
}

export interface CreateProductTag {
  productId: number;
  tagId: number;
}