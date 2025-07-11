
export interface Item {
  id: number;
  name: string;
  isWeapon: boolean;
  isActive: boolean;
  classId?: number | null;
  slotId?: number | null;
  currentStock?: number | null;
}

export interface UpdateItem {
  id: number;
  name?: string;
  isWeapon?: boolean | null;
  isActive?: boolean | null;
  classId?: number | null;
  slotId?: number | null;
  currentStock?: number | null;
}

export interface CreateItem
{
    name: string;
    isWeapon?: boolean | null;
    isActive?: boolean | null;
    classId?: number | null;
    slotId?: number | null;
    currentStock?: number | null;
}