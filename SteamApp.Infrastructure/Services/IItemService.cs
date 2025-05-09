using SteamApp.Infrastructure.DTOs;

namespace SteamApp.Infrastructure.Services
{
    public interface IItemService
    {
        Task<ItemDto> GetItemAsync(long id);
        Task<IEnumerable<ItemDto>> GetItemsAsync();

        Task<bool> CreateItemAsync(ItemDto item);

        Task<bool> DeleteItemAsync(long id);
    }
}
