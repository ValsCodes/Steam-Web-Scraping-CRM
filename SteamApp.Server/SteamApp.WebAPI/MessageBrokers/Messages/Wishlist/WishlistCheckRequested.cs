namespace SteamApp.WebAPI.MessageBrokers.Messages.Wishlist;

public sealed record WishlistCheckRequested(
    long WishlistId,
    DateTime RequestedAtUtc,
    string CorrelationId);
