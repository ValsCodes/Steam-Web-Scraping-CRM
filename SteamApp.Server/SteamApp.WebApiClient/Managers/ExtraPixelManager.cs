using SteamApp.Models.DTOs.ExtraPixel;

namespace SteamApp.WebApiClient.Managers
{
    public class ExtraPixelManager(BaseApiClient api)
    {
        public async Task<List<ExtraPixelDto>> GetAllAsync(CancellationToken ct)
        {
            return await api.GetAsync<List<ExtraPixelDto>>("api/extra-pixels", ct) ?? [];
        }

        public async Task<ExtraPixelDto?> GetByIdAsync(long id, CancellationToken ct)
        {
            return await api.GetAsync<ExtraPixelDto>($"api/extra-pixels/{id}", ct);
        }

        public async Task<ExtraPixelDto> CreateAsync(ExtraPixelCreateDto dto, CancellationToken ct)
        {
            return await api.PostAsync<ExtraPixelDto>("api/extra-pixels", dto, ct);
        }
    }
}
