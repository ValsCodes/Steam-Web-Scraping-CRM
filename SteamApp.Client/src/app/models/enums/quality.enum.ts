export enum Quality {
  Unique     = 1,
  Strange    = 2,
  Mercenary  = 3,
  Commando   = 4,
  Assassin   = 5,
  Elite      = 6,
  Genuine    = 7,
  Vintage    = 8,
}

export const qualitiesMap: Record<Quality, string> = {
  [Quality.Unique]:    'Unique',
  [Quality.Strange]:   'Strange',
  [Quality.Mercenary]: 'Mercenary',
  [Quality.Commando]:  'Commando',
  [Quality.Assassin]:  'Assassin',
  [Quality.Elite]:     'Elite',
  [Quality.Genuine]:   'Genuine',
  [Quality.Vintage]:   'Vintage',
};

export const qualitiesCollection: { id: Quality; label: string }[] = [
  { id: Quality.Unique,    label: qualitiesMap[Quality.Unique]    },
  { id: Quality.Strange,   label: qualitiesMap[Quality.Strange]   },
  { id: Quality.Mercenary, label: qualitiesMap[Quality.Mercenary] },
  { id: Quality.Commando,  label: qualitiesMap[Quality.Commando]  },
  { id: Quality.Assassin,  label: qualitiesMap[Quality.Assassin]  },
  { id: Quality.Elite,     label: qualitiesMap[Quality.Elite]     },
  { id: Quality.Genuine,   label: qualitiesMap[Quality.Genuine]   },
  { id: Quality.Vintage,   label: qualitiesMap[Quality.Vintage]   },
];