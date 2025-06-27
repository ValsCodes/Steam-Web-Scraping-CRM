using SteamApp.Models.Models;

namespace SteamApp.Infrastructure.Repositories
{
    public interface IItemRepository
    {
        Task<Item> GetItemByIdAsync(long id, CancellationToken ct);
        Task<IEnumerable<Item>> GetItemsAsync(CancellationToken ct, IEnumerable<long>? classFilters = null, IEnumerable<long>? slotFilters = null);

        Task<IEnumerable<Item>> GetItemByNameAsync(string name, CancellationToken ct);

        Task<long> CreateItemAsync(Item item, CancellationToken ct);
        Task<bool> UpdateItemAsync(Item item, CancellationToken ct);

        Task<bool> DeleteItemAsync(long id, CancellationToken ct);
    }
}
