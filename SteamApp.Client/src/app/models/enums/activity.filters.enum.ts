export enum ActivityFilters {
  Active = 1,
  Inactive = 2,
  Weapon = 3,
  NonWeapon = 4,
}

export const activityFiltersMap: Record<ActivityFilters, string> = {
  [ActivityFilters.Active]:    'Active',
  [ActivityFilters.Inactive]:   'Inactive',
  [ActivityFilters.Weapon]: 'Weapon',
  [ActivityFilters.NonWeapon]:  'Non Weapon',
};

export const activityFiltersCollection: {
  id: ActivityFilters;
  label: string;
  checked: boolean;
}[] = [
  {
    id: ActivityFilters.Active,
    label: activityFiltersMap[ActivityFilters.Active],
    checked: false,
  },
  {
    id: ActivityFilters.Inactive,
    label: activityFiltersMap[ActivityFilters.Inactive],
    checked: false,
  },
  {
    id: ActivityFilters.Weapon,
    label: activityFiltersMap[ActivityFilters.Weapon],
    checked: false,
  },
  {
    id: ActivityFilters.NonWeapon,
    label: activityFiltersMap[ActivityFilters.NonWeapon],
    checked: false,
  },
];