// src/app/models/tag.model.ts

export interface Tag {
  id: number;
  gameId: number;
  gameName:string | null;
  name: string | null;
  isActive: boolean;
  itemGroupId: number | null;
  itemGroupName: string | null;
}

export interface CreateTag {
  gameId: number;
  name: string;
  isActive: boolean;
  itemGroupId: number | null;
}

export interface UpdateTag {
  name: string;
  isActive?: boolean | null;
  itemGroupId?: number | null;
}

export interface UpdateTagStatus {
  id: number;
  isActive: boolean;
}
