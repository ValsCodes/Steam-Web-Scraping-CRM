namespace SteamApp.Contracts.Events.Wishlist;

public sealed record WishlistNotificationRequested(
    long WishlistId,
    string? WishlistName,
    string Email,
    string GameName,
    double CurrentPrice,
    DateTime RequestedAtUtc,
    string CorrelationId);
