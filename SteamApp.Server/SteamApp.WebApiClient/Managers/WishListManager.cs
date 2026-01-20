using SteamApp.Application.DTOs.WishListItem;

namespace SteamApp.WebApiClient.Managers
{
    public class WishListManager(BaseApiClient api)
    {
        public async Task<List<WishListDto>> GetAllAsync(CancellationToken ct)
        {
            return await api.GetAsync<List<WishListDto>>("api/wish-list", ct) ?? [];
        }

        public async Task<WishListDto?> GetByIdAsync(long id, CancellationToken ct)
        {
            return await api.GetAsync<WishListDto>($"api/wish-list/{id}", ct);
        }

        public async Task<WishListDto> CreateAsync(WishListCreateDto dto, CancellationToken ct)
        {
            return await api.PostAsync<WishListDto>("api/wish-list", dto, ct);
        }
    }
}
