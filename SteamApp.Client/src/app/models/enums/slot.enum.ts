export enum Slot {
  Primary = 1,
  Secondary = 2,
  Melle = 3,
  Other = 4,
}

export const slotsMap: Record<Slot, string> = {
  [Slot.Primary]: 'Primary',
  [Slot.Secondary]: 'Secondary',
  [Slot.Melle]: 'Melle',
  [Slot.Other]: 'Other',
};

export const slotsCollection: { id: Slot; label: string }[] = [
  { id: Slot.Primary, label: slotsMap[Slot.Primary] },
  { id: Slot.Secondary, label: slotsMap[Slot.Secondary] },
  { id: Slot.Melle, label: slotsMap[Slot.Melle] },
  { id: Slot.Other, label: slotsMap[Slot.Other] },
];
