using System.Diagnostics;
using Microsoft.Extensions.Caching.Distributed;
using SteamApp.Application.Caching;
using SteamApp.Application.DTOs.WishListItem;
using SteamApp.Interfaces.Services;
using SteamApp.WebAPI.Caching;
using SteamApp.WebAPI.MessageBrokers.Messages.Wishlist;
using SteamApp.WebAPI.Observability;

namespace SteamApp.WebAPI.MessageBrokers.Handlers.Wishlist;

public sealed class WishlistNotificationMessageHandler(
    ILogger<WishlistNotificationMessageHandler> logger,
    IDistributedCache cache,
    IEmailService emailService)
{
    public async Task HandleAsync(
        WishlistNotificationRequested message,
        CancellationToken cancellationToken)
    {
        var notificationCacheKey = string.Format(
            CacheKeys.WishListBackgroundJob,
            message.WishlistId);
        var queuedCacheKey = string.Format(
            CacheKeys.WishListBackgroundJobQueued,
            message.WishlistId);

        if (await cache.ExistsAsync(notificationCacheKey, cancellationToken))
        {
            SteamAppTelemetry.MarkSkipped(Activity.Current);
            await cache.RemoveAsync(queuedCacheKey, cancellationToken);
            logger.LogInformation(
                "Wishlist notification message {CorrelationId} skipped because the item is already cached.",
                message.CorrelationId);
            return;
        }

        await emailService.SendAsync(new EmailMessage(
            To: message.Email,
            Subject: $"Wishlist item {message.GameName} Price has been reached!",
            Body: $"{message.GameName} is currently at {message.CurrentPrice} EUR"), cancellationToken);

        await cache.SetJsonAsync(
            notificationCacheKey,
            new WhishListResponse
            {
                GameName = message.GameName,
                CurrentPrice = message.CurrentPrice,
                IsPriceReached = true
            },
            TimeSpan.FromHours(12),
            cancellationToken);
        await cache.RemoveAsync(queuedCacheKey, cancellationToken);

        logger.LogInformation(
            "Wishlist notification message {CorrelationId} sent an email.",
            message.CorrelationId);
    }
}
