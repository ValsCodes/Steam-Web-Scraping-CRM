export interface Game {
  id: number;
  name: string;
  baseUrl: string;
}

export interface CreateGame {
  name: string;
  baseUrl: string;
}

export interface UpdateGame {
  name: string;
  baseUrl: string;
}