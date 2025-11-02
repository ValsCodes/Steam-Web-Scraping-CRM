using SteamApp.Models.Entities;

namespace SteamApp.Infrastructure.Repositories
{
    public interface IProductRepository
    {
        Task<WatchItem> GetByIdAsync(long id, CancellationToken ct = default);
        Task<IEnumerable<WatchItem>> GetListAsync(CancellationToken ct = default);

        Task<long> CreateAsync(WatchItem product, CancellationToken ct = default);
        Task<IEnumerable<long>> CreateRangeAsync(IEnumerable<WatchItem> products, CancellationToken ct = default);

        //Task<bool> UpdateAsync(long id, Product product, CancellationToken ct = default);
        Task<bool> UpdateAsync(WatchItem product, CancellationToken ct = default);
        Task<IEnumerable<bool>> UpdateRangeAsync(IEnumerable<WatchItem> products, CancellationToken ct = default);

        Task<bool> DeleteAsync(long id, CancellationToken ct = default);
        Task<IEnumerable<bool>> DeleteRangeAsync(IEnumerable<long> ids, CancellationToken ct = default);
    }
}
