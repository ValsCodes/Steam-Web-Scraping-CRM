export interface GroupableItem {
  id: number;
  name: string | null;
  itemGroupId: number | null;
  itemGroupName: string | null;
}

export interface ItemGroupSection<T extends GroupableItem> {
  itemGroupId: number | null;
  name: string;
  items: readonly T[];
}

export function groupByItemGroup<T extends GroupableItem>(
  items: readonly T[],
): readonly ItemGroupSection<T>[] {
  const groupedItems = new Map<number, { name: string; items: T[] }>();
  const ungroupedItems: T[] = [];

  for (const item of items) {
    if (item.itemGroupId === null) {
      ungroupedItems.push(item);
      continue;
    }

    const existing = groupedItems.get(item.itemGroupId);
    if (existing) {
      existing.items.push(item);
      continue;
    }

    groupedItems.set(item.itemGroupId, {
      name: item.itemGroupName?.trim() || 'Unnamed group',
      items: [item],
    });
  }

  const sections: ItemGroupSection<T>[] = [...groupedItems.entries()]
    .map(([itemGroupId, group]) => ({
      itemGroupId,
      name: group.name,
      items: sortItems(group.items),
    }))
    .sort((left, right) =>
      left.name.localeCompare(right.name) || left.itemGroupId - right.itemGroupId,
    );

  if (ungroupedItems.length > 0) {
    sections.push({
      itemGroupId: null,
      name: 'Ungrouped',
      items: sortItems(ungroupedItems),
    });
  }

  return sections;
}

function sortItems<T extends GroupableItem>(items: readonly T[]): readonly T[] {
  return [...items].sort((left, right) =>
    (left.name ?? '').localeCompare(right.name ?? '') || left.id - right.id,
  );
}
