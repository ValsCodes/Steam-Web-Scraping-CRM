export interface Pixel {
  id: number;
  gameId: number;
  gameName: string;
  gameUrlId: number;
  gameUrlName: string;
  name: string;
  value: number;
}

export interface CreatePixel {
  gameUrlId: number;
  name: string;
  value: number;
}

export interface UpdatePixel {
  gameUrlId?: number;
  name?: string | null;
  value?: number | null;
}
