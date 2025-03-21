using SteamApp.Models.Models;
using SteamApp.Models.DTOs;

namespace SteamApp.Infrastructure.Repositories
{
    public interface IItemRepository
    {
        Task<Item> GetItemAsync(long id);
        Task<IEnumerable<Item>> GetItemsAsync();

        Task<bool> CreateItemAsync(ItemDto item);

        Task<bool> DeleteItemAsync(long id);
    }
}
