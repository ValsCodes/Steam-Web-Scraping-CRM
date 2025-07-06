export enum Class {
  Scout = 1,
  Soldier = 2,
  Pyro = 3,
  Demoman = 4,
  Heavy = 5,
  Engineer = 6,
  Medic = 7,
  Sniper = 8,
  Spy = 9,
}

export const classesMap: Record<Class, string> = {
  [Class.Scout]: 'Scout',
  [Class.Soldier]: 'Soldier',
  [Class.Pyro]: 'Pyro',
  [Class.Demoman]: 'Demoman',
  [Class.Heavy]: 'Heavy',
  [Class.Engineer]: 'Engineer',
  [Class.Medic]: 'Medic',
  [Class.Sniper]: 'Sniper',
  [Class.Spy]: 'Spy',
};

export const classesCollection: { id: Class; label: string }[] = [
  { id: Class.Scout, label: classesMap[Class.Scout] },
  { id: Class.Soldier, label: classesMap[Class.Soldier] },
  { id: Class.Pyro, label: classesMap[Class.Pyro] },
  { id: Class.Demoman, label: classesMap[Class.Demoman] },
  { id: Class.Heavy, label: classesMap[Class.Heavy] },
  { id: Class.Engineer, label: classesMap[Class.Engineer] },
  { id: Class.Medic, label: classesMap[Class.Medic] },
  { id: Class.Sniper, label: classesMap[Class.Sniper] },
  { id: Class.Spy, label: classesMap[Class.Spy] },
];
