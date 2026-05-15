export interface Listing {
  name: string;
  price: number;
  imageUrl: string;
  quantity: number;
  pixelName: string;
  linkUrl: string;
  pageUrl: string;
  redValue: number | null;
  blueValue: number | null;
  greenValue: number | null;
  isPainted: boolean;
}

export interface WhishListResponse {
  gameName: string;
  isPriceReached: boolean;
  currentPrice: number;
}