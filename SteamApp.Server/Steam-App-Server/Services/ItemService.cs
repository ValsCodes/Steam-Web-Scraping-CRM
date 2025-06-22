using AutoMapper;
using SteamApp.Infrastructure.Repositories;
using SteamApp.Infrastructure.Services;
using SteamApp.Infrastructure.DTOs; 

namespace SteamApp.Services
{
    public class ItemService : IItemService
    {
        private readonly IItemRepository _itemRepository;
        private readonly IMapper _mapper;

        public ItemService(IItemRepository itemRepository, IMapper mapper)
        {
            _itemRepository = itemRepository;
            _mapper = mapper;
        }

        public async Task<ItemDto> GetItemAsync(long id)
        {
            throw new NotImplementedException();
        }

        public async Task<IEnumerable<ItemDto>> GetItemsAsync()
        {
            var items = await _itemRepository.GetItemsAsync();

            var mappedItem = _mapper.Map<List<ItemDto>>(items);

            return mappedItem;
        }

        public async Task<bool> CreateItemAsync(ItemDto item)
        {

            throw new NotImplementedException();
        }

        public async Task<bool> DeleteItemAsync(long id)
        {
            throw new NotImplementedException();
        }
    
    }
}
