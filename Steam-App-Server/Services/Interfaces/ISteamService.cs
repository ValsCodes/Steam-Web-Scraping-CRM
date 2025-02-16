using SteamAppServer.Models.DTO;
using SteamAppServer.Models.Proxies;

namespace SteamAppServer.Services.Interfaces
{
    public interface ISteamService
    {
        #region Steam Web Scraper
        Task<ProductProxy[]> GetFilterredListingsAsync(short page);
        Task<IEnumerable<ProductProxy>> GetPaintedListingsOnlyAsync(short page);
        Task<(bool, string)> IsListingPaintedAsync(string name);
        #endregion

        Task<ProductDto> GetProductAsync(long? id);
        Task<IEnumerable<ProductDto>> GetProductsAsync();

        Task<ProductDto> CreateProductAsync(ProductDto? product);
        Task<IEnumerable<ProductDto>> CreateProductsAsync(ProductDto[] products);

        Task<bool> UpdateProductAsync(ProductDto? product);
        Task<bool[]> UpdateProductsAsync(ProductDto[] products);

        Task<bool> DeleteProductAsync(long? id);
        Task<bool[]> DeleteProductsAsync(long[] ids);
        Task<string[]> GetWeaponListingsUrls(short fromIndex, short batchSize);

        Task<string[]> GetHatListingsUrls(short fromPage, short batchSize);
    }
}
