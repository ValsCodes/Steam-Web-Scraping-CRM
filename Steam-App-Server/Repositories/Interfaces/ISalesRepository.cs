using SteamAppServer.Models;

namespace SteamAppServer.Repositories.Interfaces
{
    public interface ISalesRepository
    {
        Task<Product?> GetProductAsync(long id);
        Task<IEnumerable<Product>> GetProductsAsync();
        Task<Product?> CreateProductAsync(Product product);
        Task<IEnumerable<Product>> CreateProductsAsync(Product[] products);
        Task<Product?> UpdateProductAsync(Product product);
        Task<IEnumerable<Product>> UpdateProductsAsync(Product[] products);
        Task<long?> DeleteProductAsync(long id);
        Task<IEnumerable<long?>> DeleteProductsAsync(long[] ids);
    }
}
