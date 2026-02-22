using SteamApp.Application.DTOs.Pixel;

namespace SteamApp.WebApiClient.Managers
{
    public class ExtraPixelManager(BaseApiClient api)
    {
        public async Task<List<PixelDto>> GetAllAsync(CancellationToken ct)
        {
            return await api.GetAsync<List<PixelDto>>("api/extra-pixels", ct) ?? [];
        }

        public async Task<PixelDto?> GetByIdAsync(long id, CancellationToken ct)
        {
            return await api.GetAsync<PixelDto>($"api/extra-pixels/{id}", ct);
        }

        public async Task<PixelDto> CreateAsync(PixelCreateDto dto, CancellationToken ct)
        {
            return await api.PostAsync<PixelDto>("api/extra-pixels", dto, ct);
        }
    }
}
