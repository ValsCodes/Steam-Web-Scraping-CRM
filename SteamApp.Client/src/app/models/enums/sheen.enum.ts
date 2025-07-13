export enum Sheen {
  HotRod = 1,
  VillainousViolet = 2,
  TeamShine = 3,
  MeanGreen = 4,
  AgonizingEmerald = 5,
  Mandarin = 6,
  DeadlyDaffodil = 7,
}

export const sheensMap: Record<Sheen, string> = {
  [Sheen.HotRod]: 'Hot Rod',
  [Sheen.VillainousViolet]: 'Villainous Violet',
  [Sheen.TeamShine]: 'Team Shine',
  [Sheen.MeanGreen]: 'Mean Green',
  [Sheen.AgonizingEmerald]: 'Agonizing Emerald',
  [Sheen.Mandarin]: 'Mandarin',
  [Sheen.DeadlyDaffodil]: 'Deadly Daffodil',
};

export const sheensCollection: { id: Sheen; label: string }[] = [
  { id: Sheen.HotRod,           label: sheensMap[Sheen.HotRod] },
  { id: Sheen.VillainousViolet, label: sheensMap[Sheen.VillainousViolet] },
  { id: Sheen.TeamShine,        label: sheensMap[Sheen.TeamShine] },
  { id: Sheen.MeanGreen,        label: sheensMap[Sheen.MeanGreen] },
  { id: Sheen.AgonizingEmerald, label: sheensMap[Sheen.AgonizingEmerald] },
  { id: Sheen.Mandarin,         label: sheensMap[Sheen.Mandarin] },
  { id: Sheen.DeadlyDaffodil,   label: sheensMap[Sheen.DeadlyDaffodil] },
];
