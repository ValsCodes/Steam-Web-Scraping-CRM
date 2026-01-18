export interface Pixel {
  id: number;
  gameUrlId: number;
  value: number;
}

export interface CreatePixel {
  gameUrlId: number;
  value: number;
}

export interface UpdatePixel {
  value: number;
}
