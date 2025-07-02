using SteamApp.Models.Entities;

namespace SteamApp.Infrastructure.Repositories;

public interface IItemRepository
{
    Task<Item> GetItemByIdAsync(long id, CancellationToken ct);
    Task<IEnumerable<Item>> GetItemsAsync(CancellationToken ct, string? name = null, IEnumerable<long>? classFilters = null, IEnumerable<long>? slotFilters = null);

    Task<long> CreateItemAsync(Item item, CancellationToken ct);
    Task<bool> UpdateItemAsync(Item item, CancellationToken ct);

    Task<bool> DeleteItemAsync(long id, CancellationToken ct);
}
