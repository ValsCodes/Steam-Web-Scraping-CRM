export interface Listing {
  name: string;
  price: number;
  imageUrl: string;
  quantity: number;
  color: string;
  linkUrl: string;
}

export interface WhishListResponse {
  gameName: string;
  isPriceReached: boolean;
  currentPrice: number;
}