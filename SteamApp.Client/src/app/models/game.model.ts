export interface Game {
  id: number;
  name: string;
  baseUrl: string;
  pageUrl: string | null;
  internalId: number | null;
}

export interface CreateGame {
  name: string;
  baseUrl: string;
  pageUrl: string | null;
  internalId: number | null;
}

export interface UpdateGame {
  name: string;
  baseUrl: string;
  pageUrl: string | null;
  internalId: number | null;
}