export function makeEnumHelpers<E extends { [k: string]: string | number }>(e: E) {
  const names = Object.keys(e)
    .filter(k => typeof e[k] === 'number') as (keyof E)[]
  
  const map = names.reduce<Record<number, string>>((acc, name) => {
    const val = e[name] as unknown as number
    acc[val] = name as string
    return acc
  }, {})

  const collection = names.map(name => ({
    id: e[name] as unknown as number,
    label: name as string,
  }))

  return { map, collection } as {
    map: Record<E[keyof E] & number, string>;
    collection: Array<{ id: E[keyof E] & number; label: string }>;
  }
}
