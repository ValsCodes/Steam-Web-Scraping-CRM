using SteamApp.Models.DTOs.Game;

namespace SteamApp.WebApiClient.Managers;

public class GameManager(BaseApiClient api)
{
    public async Task<List<GameDto>> GetAllAsync(CancellationToken ct)
    {
        return await api.GetAsync<List<GameDto>>("api/games", ct) ?? [];
    }

    public async Task<GameDto?> GetByIdAsync(long id, CancellationToken ct)
    {
        return await api.GetAsync<GameDto>($"api/games/{id}", ct);
    }

    public async Task<GameDto> CreateAsync(GameCreateDto dto, CancellationToken ct)
    {
        return await api.PostAsync<GameDto>("api/games", dto, ct);
    }
}
