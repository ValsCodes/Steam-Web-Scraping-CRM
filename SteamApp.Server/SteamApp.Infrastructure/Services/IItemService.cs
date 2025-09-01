using SteamApp.Models.DTOs.Item;
using SteamApp.Models.OperationResults;

namespace SteamApp.Infrastructure.Services;

public interface IItemService
{
    Task<ItemDto> GetItemById(long id, CancellationToken ct);

    Task<IEnumerable<ItemDto>> GetItems(CancellationToken ct, string? name = null, IEnumerable<long>? classFilters = null, IEnumerable<long>? slotFilters = null);

    Task<BaseOperationResult> CreateItemAsync(CreateItemDto item, CancellationToken ct);
    Task<BaseOperationResult> UpdateItem(ItemDto item, CancellationToken ct);

    Task<BaseOperationResult> DeleteById(long id, CancellationToken ct);
}
