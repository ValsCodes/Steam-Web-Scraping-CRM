// game-url-pixel.model.ts
export interface GameUrlPixel {
  pixelId: number;
  pixelName: string;

  gameUrlId: number;
  gameUrlName: string;

  gameUrlPixelLocationX: number;
  gameUrlPixelLocationY: number;
  gameUrlImageWidth: number;
  gameUrlImageHeight: number;

  redValue: number;
  greenValue: number;
  blueValue: number;
}

// create-game-url-pixel.model.ts
export interface CreateGameUrlPixel {
  pixelId: number;
  gameUrlId: number;
}
