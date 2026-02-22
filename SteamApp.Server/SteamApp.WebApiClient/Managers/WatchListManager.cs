using SteamApp.Application.DTOs.WatchList;
using SteamApp.Application.DTOs.WatchListItem;

namespace SteamApp.WebApiClient.Managers
{
    public class WatchListManager(BaseApiClient api)
    {
        public async Task<List<WatchListDto>> GetAllAsync(CancellationToken ct)
        {
            return await api.GetAsync<List<WatchListDto>>("api/watch-list", ct) ?? [];
        }

        public async Task<WatchListDto?> GetByIdAsync(long id, CancellationToken ct)
        {
            return await api.GetAsync<WatchListDto>($"api/watch-list/{id}", ct);
        }

        public async Task<WatchListDto> CreateAsync(WatchListCreateDto dto, CancellationToken ct)
        {
            return await api.PostAsync<WatchListDto>("api/watch-list", dto, ct);
        }
    }
}
