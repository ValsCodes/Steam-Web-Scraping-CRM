using SteamApp.Models.DTOs.Item;

namespace SteamApp.Infrastructure.Services;

public interface IItemService
{
    Task<ItemDto> GetItemById(long id, CancellationToken ct);

    Task<IEnumerable<ItemDto>> GetItems(CancellationToken ct, string? name = null, IEnumerable<long>? classFilters = null, IEnumerable<long>? slotFilters = null);

    Task<OperationResult> CreateItemAsync(CreateItemDto item, CancellationToken ct);
    Task<OperationResult> UpdateItem(ItemDto item, CancellationToken ct);

    Task<OperationResult> DeleteById(long id, CancellationToken ct);
}
