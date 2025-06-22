using SteamApp.Models;

namespace SteamApp.Infrastructure.Repositories
{
    public interface IProductRepository
    {
        Task<Product> GetByIdAsync(long id, CancellationToken ct = default);
        Task<IEnumerable<Product>> GetListAsync(CancellationToken ct = default);

        Task<long> CreateAsync(Product product, CancellationToken ct = default);
        Task<IEnumerable<long>> CreateRangeAsync(IEnumerable<Product> products, CancellationToken ct = default);

        //Task<bool> UpdateAsync(long id, Product product, CancellationToken ct = default);
        Task<bool> UpdateAsync(Product product, CancellationToken ct = default);
        Task<IEnumerable<bool>> UpdateRangeAsync(IEnumerable<Product> products, CancellationToken ct = default);

        Task<bool> DeleteAsync(long id, CancellationToken ct = default);
        Task<IEnumerable<bool>> DeleteRangeAsync(IEnumerable<long> ids, CancellationToken ct = default);
    }
}
