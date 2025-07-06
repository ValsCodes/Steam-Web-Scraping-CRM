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

const classIds = Object.values(Class)
  .filter((v): v is Class => typeof v === 'number')

export const classesMap = Object.fromEntries(
  classIds.map(id => [id, Class[id]])
) as Record<Class, string>

export const classesCollection = classIds.map(id => ({
  id,
  label: Class[id],
}))
