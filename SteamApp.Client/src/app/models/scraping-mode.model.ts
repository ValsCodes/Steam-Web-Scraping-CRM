export interface ScrapingMode {
  id: number;
  name: string;
}

export enum ScrapingModeEnum {
  ManualBatch = 1,
  Batch = 2,
  PixelBatch = 3,
  PublicApi = 4,
}

export interface CreateScrapingMode {
  name: string;
}

export interface UpdateScrapingMode {
  name: string;
}
