
export interface ISellListing {
    id: number;
    name?: string;
    description?: string;
    dateBought: Date;
    dateSold?: Date;
    costPrice: number;
    targetSellPrice1: number;
    targetSellPrice2?: number;
    targetSellPrice3?: number;
    targetSellPrice4?: number;
    soldPrice?: number;
    isHat: boolean;
    isWeapon: boolean;
    isSold: boolean;
}