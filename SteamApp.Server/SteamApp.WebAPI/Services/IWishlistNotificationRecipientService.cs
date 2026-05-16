namespace SteamApp.WebAPI.Services;

public interface IWishlistNotificationRecipientService
{
    Task<IReadOnlyList<WishlistNotificationRecipient>> GetActiveRecipientsAsync(
        CancellationToken cancellationToken);
}

public sealed record WishlistNotificationRecipient(
    long WishlistId,
    string? WishlistName,
    string Email);
