export interface Pixel {
  id: number;
  name: string;
  redValue: number;
  greenValue: number;
  blueValue: number;
  gameId: number;
}

export interface PixelListItem extends Pixel {
  gameName: string;
}

export interface CreatePixel {
  name: string;
  redValue: number;
  greenValue: number;
  blueValue: number;
  gameId: number;
}

export interface UpdatePixel {
  name: string;
  redValue: number;
  greenValue: number;
  blueValue: number;
  gameId: number;
}
