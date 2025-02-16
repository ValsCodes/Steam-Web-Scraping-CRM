
export interface Product {
    id: number;
    name: string;
    qualityId: number | null;
    description: string;
    dateBought: Date | null;
    dateSold: Date | null;
    boughtPrice : number | null;
    targetSellPrice1: number | null;
    targetSellPrice2: number | null;
    targetSellPrice3: number;
    targetSellPrice4: number;
    soldPrice: number | null;
    isHat: boolean;
    isWeapon: boolean;
    isSold: boolean;
}