using SteamApp.Infrastructure.DTOs.Item;

namespace SteamApp.Infrastructure.Services;

public interface IItemService
{
    Task<ItemDto> GetItemByIdAsync(long id, CancellationToken ct);

    Task<IEnumerable<ItemDto>> GetItemsAsync(CancellationToken ct, string? name = null, IEnumerable<long>? classFilters = null, IEnumerable<long>? slotFilters = null);

    Task<OperationResult> CreateItemAsync(CreateItemDto item, CancellationToken ct);
    Task<OperationResult> UpdateItemAsync(ItemDto item, CancellationToken ct);

    Task<OperationResult> DeleteItemAsync(long id, CancellationToken ct);
}
