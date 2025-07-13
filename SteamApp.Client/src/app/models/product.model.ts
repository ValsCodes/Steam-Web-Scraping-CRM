export interface Product {
  id: number;
  name: string;
  qualityId: number | null;
  description: string;
  dateBought: Date | null;
  dateSold: Date | null;
  costPrice: number | null;
  targetSellPrice1: number | null;
  targetSellPrice2: number | null;
  targetSellPrice3: number | null;
  targetSellPrice4: number | null;
  soldPrice: number | null;
  isHat: boolean;
  isWeapon: boolean;
  isSold: boolean;
  isStrange: boolean | null;
  paintId: number | null;
  sheenId: number | null;
}

export interface UpdateProduct {
  id: number;
  name?: string;
  qualityId?: number | null;
  description?: string;
  dateBought?: Date | null;
  dateSold?: Date | null;
  costPrice?: number | null;
  targetSellPrice1?: number | null;
  targetSellPrice2?: number | null;
  targetSellPrice3?: number | null;
  targetSellPrice4?: number | null;
  soldPrice?: number | null;
  isHat?: boolean;
  isWeapon?: boolean;
  isSold?: boolean;
  isStrange?: boolean | null;
  paintId?: number | null;
  sheenId?: number | null;
}

export interface CreateProduct {
  name: string;
  qualityId?: number | null;
  description?: string | null;
  dateBought?: Date | null;
  dateSold?: Date | null;
  costPrice?: number | null;
  targetSellPrice1?: number | null;
  targetSellPrice2?: number | null;
  targetSellPrice3?: number | null;
  targetSellPrice4?: number | null;
  soldPrice?: number | null;
  isHat?: boolean | null;
  isWeapon?: boolean | null;
  isSold?: boolean | null;
  isStrange?: boolean | null;
  paintId?: number | null;
  sheenId?: number | null;
}
