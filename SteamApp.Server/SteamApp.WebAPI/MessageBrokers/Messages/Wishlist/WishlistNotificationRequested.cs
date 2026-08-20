namespace SteamApp.WebAPI.MessageBrokers.Messages.Wishlist;

public sealed record WishlistNotificationRequested(
    long WishlistId,
    string? WishlistName,
    string Email,
    string GameName,
    double CurrentPrice,
    DateTime RequestedAtUtc,
    string CorrelationId);
