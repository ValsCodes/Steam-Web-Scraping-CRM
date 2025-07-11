using SteamApp.Models.Entities;

namespace SteamApp.Infrastructure.Repositories;

public interface IItemRepository
{
    Task<ManualSearchItem> GetItemById(long id, CancellationToken ct);
    Task<IEnumerable<ManualSearchItem>> GetItems(CancellationToken ct, string? name = null, IEnumerable<long>? classFilters = null, IEnumerable<long>? slotFilters = null);

    Task<long> CreateItem(ManualSearchItem item, CancellationToken ct);
    Task<bool> UpdateItem(ManualSearchItem item, CancellationToken ct);

    Task<bool> DeleteItem(long id, CancellationToken ct);
}
