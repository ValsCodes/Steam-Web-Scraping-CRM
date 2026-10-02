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

        public async Task<WishListCheckHistoryDto> CheckAsync(long id, CancellationToken ct)
        {
            return await api.PostAsync<WishListCheckHistoryDto>(
                $"api/wish-list/{id}/checks",
                new { },
                ct);
        }

        public async Task<WishListCheckHistoryPageDto?> GetCheckHistoryAsync(
            long id,
            int pageNumber = 1,
            int pageSize = 25,
            CancellationToken ct = default)
        {
            return await api.GetAsync<WishListCheckHistoryPageDto>(
                $"api/wish-list/{id}/checks?pageNumber={pageNumber}&pageSize={pageSize}",
                ct);
        }
    }
}
