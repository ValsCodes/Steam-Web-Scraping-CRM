using SteamApp.Models.Entities;

namespace SteamApp.Infrastructure.Repositories;

public interface IItemRepository
{
    Task<Item> GetItemById(long id, CancellationToken ct);
    Task<IEnumerable<Item>> GetItems(CancellationToken ct, string? name = null, IEnumerable<long>? classFilters = null, IEnumerable<long>? slotFilters = null);

    Task<long> CreateItem(Item item, CancellationToken ct);
    Task<bool> UpdateItem(Item item, CancellationToken ct);

    Task<bool> DeleteItem(long id, CancellationToken ct);
}
