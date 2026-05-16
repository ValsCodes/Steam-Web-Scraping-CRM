using Microsoft.EntityFrameworkCore;
using SteamApp.Infrastructure.Context;

namespace SteamApp.WebAPI.Services;

public sealed class WishlistNotificationRecipientService(
    IDbContextFactory<ApplicationDbContext> dbContextFactory)
    : IWishlistNotificationRecipientService
{
    public async Task<IReadOnlyList<WishlistNotificationRecipient>> GetActiveRecipientsAsync(
        CancellationToken cancellationToken)
    {
        await using var db = dbContextFactory.CreateDbContext();

        return await db.WishLists
            .AsNoTracking()
            .Where(wishlist => wishlist.IsActive && wishlist.UserId != null)
            .Join(
                db.Users.AsNoTracking().Where(user => user.Email != null && user.Email != string.Empty),
                wishlist => wishlist.UserId,
                user => user.Id,
                (wishlist, user) => new WishlistNotificationRecipient(
                    wishlist.Id,
                    wishlist.Name,
                    user.Email!))
            .ToListAsync(cancellationToken);
    }
}
