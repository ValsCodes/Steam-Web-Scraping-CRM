using SteamApp.Models.Dto;

namespace SteamApp.Infrastructure.Services
{
    public interface IProductService
    {
        Task<ProductDto> GetProductAsync(long? id);
        Task<IEnumerable<ProductDto>> GetProductsAsync();

        Task<ProductDto> CreateProductAsync(ProductDto? product);
        Task<IEnumerable<ProductDto>> CreateProductsAsync(ProductDto[] products);

        Task<bool> UpdateProductAsync(ProductDto? product);
        Task<bool[]> UpdateProductsAsync(ProductDto[] products);

        Task<bool> DeleteProductAsync(long? id);
        Task<bool[]> DeleteProductsAsync(long[] ids);
    }
}
