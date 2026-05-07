export interface Game {
  id: number;
  name: string;
  baseUrl: string;
  pageUrl: string | null;
  internalId: number | null;
  isActive: boolean;
}

export interface CreateGame {
  name: string;
  baseUrl: string;
  pageUrl: string | null;
  internalId: number | null;
  isActive: boolean;
}

export interface UpdateGame {
  name: string;
  baseUrl: string;
  pageUrl: string | null;
  internalId: number | null;
  isActive?: boolean | null;
}

export interface UpdateGameStatus {
  id: number;
  isActive: boolean;
}
