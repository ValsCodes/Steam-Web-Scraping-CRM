using Microsoft.EntityFrameworkCore;
using SteamApp.Application.DTOs.WishListItem;
using SteamApp.Infrastructure.Context;
using SteamApp.Interfaces.Services;

namespace SteamApp.WebAPI.Services;

public sealed class WishlistNotificationRecipientService(
    IDbContextFactory<ApplicationDbContext> dbContextFactory)
    : IWishlistNotificationRecipientService
{
    public async Task<IReadOnlyList<WishlistNotificationRecipient>> GetActiveRecipientsAsync(
        CancellationToken cancellationToken)
    {
        await using var db = dbContextFactory.CreateDbContext();

        return await ActiveRecipients(db)
            .ToListAsync(cancellationToken);
    }

    public async Task<WishlistNotificationRecipient?> GetActiveRecipientAsync(
        long wishlistId,
        CancellationToken cancellationToken)
    {
        await using var db = dbContextFactory.CreateDbContext();

        return await ActiveRecipients(db)
            .SingleOrDefaultAsync(recipient => recipient.WishlistId == wishlistId, cancellationToken);
    }

    private static IQueryable<WishlistNotificationRecipient> ActiveRecipients(ApplicationDbContext db)
    {
        return db.WishLists
            .AsNoTracking()
            .Where(wishlist => wishlist.IsActive && wishlist.UserId != null)
            .Join(
                db.Users.AsNoTracking().Where(user => user.Email != null && user.Email != string.Empty),
                wishlist => wishlist.UserId,
                user => user.Id,
                (wishlist, user) => new WishlistNotificationRecipient(
                    wishlist.Id,
                    wishlist.Name,
                    user.Email!));
    }
}
