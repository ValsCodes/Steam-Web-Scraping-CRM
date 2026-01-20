
using SteamApp.Application.DTOs.GameUrl;

namespace SteamApp.WebApiClient.Managers
{
    public class GameUrlManager(BaseApiClient api)
    {
        public async Task<List<GameUrlDto>> GetAllAsync(CancellationToken ct)
        {
            return await api.GetAsync<List<GameUrlDto>>("api/game-urls", ct) ?? [];
        }

        public async Task<GameUrlDto?> GetByIdAsync(long id, CancellationToken ct)
        {
            return await api.GetAsync<GameUrlDto>($"api/game-urls/{id}", ct);
        }

        public async Task<GameUrlDto> CreateAsync(GameUrlCreateDto dto, CancellationToken ct)
        {
            return await api.PostAsync<GameUrlDto>("api/game-urls", dto, ct);
        }
    }
}
