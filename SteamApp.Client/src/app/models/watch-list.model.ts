export interface WatchList {
  id: number;
  url?: string | null;
  name?: string | null;
  registrationDate: string;
  isActive: boolean;
}

export interface CreateWatchList {
  url?: string | null;
  name?: string | null;
  registrationDate: string;
  isActive?: boolean | null;
}

export interface UpdateWatchList {
  name?: string | null;
  url?: string | null;
  registrationDate: string;
  isActive: boolean;
}
