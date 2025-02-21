using SteamApp.Models;
using SteamApp.Models.Dto;

namespace SteamApp.Infrastructure.Repositories
{
    public interface IProductRepository
    {
        Task<Product> GetProductAsync(long id);
        Task<IEnumerable<Product>> GetProductsAsync();

        Task<Product> CreateProductAsync(ProductDto product);
        Task<IEnumerable<Product>> CreateProductsAsync(ProductDto[] products);

        Task<bool> UpdateProductAsync(ProductDto product);
        Task<bool[]> UpdateProductsAsync(ProductDto[] products);

        Task<bool> DeleteProductAsync(long id);
        Task<bool[]> DeleteProductsAsync(long[] ids);
    }
}
