// src/app/models/tag.model.ts

export interface Tag {
  id: number;
  gameId: number;
  gameName:string | null;
  name: string | null;
}

export interface CreateTag {
  gameId: number;
  name: string;
}

export interface UpdateTag {
  name: string;
}