using SteamApp.Application.DTOs.Product;

namespace SteamApp.WebApiClient.Managers
{
    public class ProductManager(BaseApiClient api)
    {
        public async Task<List<ProductDto>> GetAllAsync(CancellationToken ct)
        {
            return await api.GetAsync<List<ProductDto>>("api/products", ct) ?? [];
        }

        public async Task<ProductDto?> GetByIdAsync(long id, CancellationToken ct)
        {
            return await api.GetAsync<ProductDto>($"api/products/{id}", ct);
        }

        public async Task<ProductDto> CreateAsync(ProductCreateDto dto, CancellationToken ct)
        {
            return await api.PostAsync<ProductDto>("api/products", dto, ct);
        }
    }
}
