using SteamApp.Infrastructure.DTOs;
using SteamApp.Infrastructure.Models;

namespace SteamApp.Infrastructure.Repositories
{
    public interface IItemRepository
    {
        Task<IItem> GetItemByIdAsync(long id);
        Task<IEnumerable<IItem>> GetItemsAsync();

        Task<bool> CreateItemAsync(ItemDto item);

        Task<bool> DeleteItemAsync(long id);
    }
}
