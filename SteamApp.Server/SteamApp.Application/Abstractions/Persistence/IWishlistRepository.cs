using SteamApp.Domain.Entities;

namespace SteamApp.Application.Abstractions.Persistence
{   
    public interface IWishlistRepository
    {
        Task<WishList?> GetAsync(long id, CancellationToken ct);

        Task<IEnumerable<WishList>> GetAllAsync(CancellationToken ct);
    }
}
