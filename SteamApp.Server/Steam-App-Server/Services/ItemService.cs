using AutoMapper;
using SteamApp.Infrastructure;
using SteamApp.Infrastructure.DTOs.Item;
using SteamApp.Infrastructure.Repositories;
using SteamApp.Infrastructure.Services;
using SteamApp.Models.Models;

namespace SteamApp.Services
{
    public class ItemService(IItemRepository itemRepository, IMapper mapper) : IItemService
    {
        public async Task<ItemDto> GetItemByIdAsync(long id, CancellationToken ct)
        {
            if (id <= 0)
                throw new InvalidCastException();

            var item = await itemRepository.GetItemByIdAsync(id, ct);

            if (item == null)
                throw new InvalidCastException();

            return mapper.Map<ItemDto>(item);
        }

        public async Task<IEnumerable<ItemDto>> GetItemByNameAsync(string name, CancellationToken ct)
        {
            if (string.IsNullOrEmpty(name))
                throw new InvalidCastException();

            var items = await itemRepository.GetItemByNameAsync(name, ct);

            if (items == null)
                throw new InvalidCastException();

            return mapper.Map<List<ItemDto>>(items);
        }

        public async Task<IEnumerable<ItemDto>> GetItemsAsync(CancellationToken ct, IEnumerable<long>? classFilters = null, IEnumerable<long>? slotFilters = null)
        {
            var items = await itemRepository.GetItemsAsync(ct, classFilters, slotFilters);

            return mapper.Map<List<ItemDto>>(items);
        }

        public async Task<OperationResult> CreateItemAsync(CreateItemDto itemDto, CancellationToken ct)
        {
            if (itemDto == null)
                throw new ArgumentNullException(nameof(itemDto));

            var item = mapper.Map<Item>(itemDto);

            var id = await itemRepository.CreateItemAsync(item, ct);

            return new OperationResult
            {
                Id = id,
                Success = id > 0,
                Message = $"Item {id} created successfully"
            };
        }

        public async Task<OperationResult> UpdateItemAsync(ItemDto itemDto, CancellationToken ct)
        {
            if (itemDto == null)
                throw new ArgumentNullException(nameof(itemDto));

            var item = mapper.Map<Item>(itemDto);

            var result = await itemRepository.UpdateItemAsync(item, ct);

            return new OperationResult
            {
                Id = item.Id,
                Success = result,
                Message = result ? $"Item {item.Id} updated successfully" : $"Item {{item.Id}} was not updated"
            };
        }

        public async Task<OperationResult> DeleteItemAsync(long id, CancellationToken ct)
        {
            var success = await itemRepository.DeleteItemAsync(id, ct);

            return new OperationResult
            {
                Id = id,
                Success = success,
                Message = success
                    ? $"Item {id} deleted successfully"
                    : $"Item {id} not found"
            };
        }
    
    }
}
