export enum ActivityFilters {
  Active = 1,
  Inactive = 2,
  Weapon = 3,
  NonWeapon = 4,
}

const activityFilterIds = Object
  .values(ActivityFilters)
  .filter((v): v is ActivityFilters => typeof v === 'number');

export const activityFiltersMap = Object.fromEntries(
  activityFilterIds.map(id => [id, ActivityFilters[id]])
) as Record<ActivityFilters, string>;

export const activityFiltersCollection = activityFilterIds.map(id => ({
  id,
  label: activityFiltersMap[id],
  checked: false,
})) as {
  id: ActivityFilters;
  label: string;
  checked: boolean;
}[];