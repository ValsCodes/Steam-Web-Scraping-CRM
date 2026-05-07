// src/app/models/tag.model.ts

export interface Tag {
  id: number;
  gameId: number;
  gameName:string | null;
  name: string | null;
  isActive: boolean;
}

export interface CreateTag {
  gameId: number;
  name: string;
  isActive: boolean;
}

export interface UpdateTag {
  name: string;
  isActive?: boolean | null;
}

export interface UpdateTagStatus {
  id: number;
  isActive: boolean;
}
