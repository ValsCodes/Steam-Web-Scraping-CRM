import { groupByItemGroup, GroupableItem } from './item-grouping';

describe('groupByItemGroup', () => {
  it('sorts grouped items by group and item name and puts ungrouped items last', () => {
    const items: GroupableItem[] = [
      item(4, 'Zulu', null, null),
      item(3, 'Beta', 2, 'Priority'),
      item(2, 'Alpha', 2, 'Priority'),
      item(1, 'Delta', 1, 'Category'),
    ];

    const groups = groupByItemGroup(items);

    expect(groups.map((group) => group.name)).toEqual(['Category', 'Priority', 'Ungrouped']);
    expect(groups[1].items.map((groupedItem) => groupedItem.name)).toEqual(['Alpha', 'Beta']);
    expect(groups[2].items.map((groupedItem) => groupedItem.name)).toEqual(['Zulu']);
  });
});

function item(
  id: number,
  name: string,
  itemGroupId: number | null,
  itemGroupName: string | null,
): GroupableItem {
  return {
    id,
    name,
    itemGroupId,
    itemGroupName,
  };
}
