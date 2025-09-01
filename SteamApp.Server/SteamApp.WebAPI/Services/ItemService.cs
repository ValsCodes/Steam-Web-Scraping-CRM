using AutoMapper;
using SteamApp.Infrastructure.Repositories;
using SteamApp.Infrastructure.Services;
using SteamApp.Models.DTOs.Item;
using SteamApp.Models.Entities;
using SteamApp.Models.OperationResults;

namespace SteamApp.WebAPI.Services;

public class ItemService(IItemRepository itemRepository, IMapper mapper) : IItemService
{
    public async Task<ItemDto> GetItemById(long id, CancellationToken ct)
    {
        if (id <= 0)
            throw new InvalidCastException();

        var item = await itemRepository.GetItemById(id, ct);

        if (item == null)
            throw new InvalidCastException();

        return mapper.Map<ItemDto>(item);
    }


    public async Task<IEnumerable<ItemDto>> GetItems(CancellationToken ct, string? name = null, IEnumerable<long>? classFilters = null, IEnumerable<long>? slotFilters = null)
    {
        var items = await itemRepository.GetItems(ct, name, classFilters, slotFilters);

        return mapper.Map<List<ItemDto>>(items);
    }

    public async Task<BaseOperationResult> CreateItemAsync(CreateItemDto itemDto, CancellationToken ct)
    {
        if (itemDto == null)
            throw new ArgumentNullException(nameof(itemDto));

        var item = mapper.Map<Item>(itemDto);

        var id = await itemRepository.CreateItem(item, ct);

        return new BaseOperationResult
        {
            Id = id,
            Success = id > 0,
            Message = $"Item {id} created successfully"
        };
    }

    public async Task<BaseOperationResult> UpdateItem(ItemDto itemDto, CancellationToken ct)
    {
        if (itemDto == null)
            throw new ArgumentNullException(nameof(itemDto));

        var item = mapper.Map<Item>(itemDto);

        var result = await itemRepository.UpdateItem(item, ct);

        return new BaseOperationResult
        {
            Id = item.Id,
            Success = result,
            Message = result ? $"Item {item.Id} updated successfully" : $"Item {{item.Id}} was not updated"
        };
    }

    public async Task<BaseOperationResult> DeleteById(long id, CancellationToken ct)
    {
        var success = await itemRepository.DeleteItem(id, ct);

        return new BaseOperationResult
        {
            Id = id,
            Success = success,
            Message = success
                ? $"Item {id} deleted successfully"
                : $"Item {id} not found"
        };
    }

}
