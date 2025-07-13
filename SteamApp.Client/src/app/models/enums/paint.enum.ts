export enum Paint {
  Lime = 1,
  White = 2,
  TeamColor = 3,
  Black = 4,
  Pink = 5,
  Yellow = 6,
  Mint = 7
}

export const paintsMap: Record<Paint, string> = {
  [Paint.Lime]:    'Lime',
  [Paint.White]:   'White',
  [Paint.TeamColor]: 'Team Color',
  [Paint.Black]:  'Black',
  [Paint.Pink]:  'Pink',
  [Paint.Yellow]: 'Yellow',
  [Paint.Mint]: 'Mint'
};

export const paintsCollection: { id: Paint; label: string }[] = [
  { id: Paint.Lime,    label: paintsMap[Paint.Lime]    },
  { id: Paint.White,   label: paintsMap[Paint.White]   },
  { id: Paint.TeamColor, label: paintsMap[Paint.TeamColor] },
  { id: Paint.Black,  label: paintsMap[Paint.Black]  },
  { id: Paint.Pink,  label: paintsMap[Paint.Pink]  },
  { id: Paint.Yellow,     label: paintsMap[Paint.Yellow]     },
  { id: Paint.Mint,     label: paintsMap[Paint.Mint]     }
];