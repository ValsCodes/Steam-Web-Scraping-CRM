using SteamApp.Domain.Entities;

namespace SteamApp.Infrastructure.Repositories
{
    public interface IWishlistRepository
    {
        Task<WishList?> GetAsync(long id, CancellationToken ct);

        Task<IEnumerable<WishList>> GetAllAsync(CancellationToken ct);
    }
}
