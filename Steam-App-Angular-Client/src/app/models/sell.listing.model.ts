
export interface ISellListing {
    id: number;
    name: string;
    qualityId: number | null;
    description: string;
    dateBought: Date | null;
    dateSold: Date | null;
    costPrice: number;
    targetSellPrice1: number;
    targetSellPrice2: number;
    targetSellPrice3: number;
    targetSellPrice4: number;
    soldPrice: number | null;
    isHat: boolean;
    isWeapon: boolean;
    isSold: boolean;
}