using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Options;
using SteamApp.Application.Caching;
using SteamApp.Interfaces.Services;
using SteamApp.WebAPI.Caching;
using SteamApp.WebAPI.MessageBrokers.Abstractions;
using SteamApp.WebAPI.MessageBrokers.Messages.Wishlist;
using SteamApp.WebAPI.MessageBrokers.Providers.RabbitMq.Options;

namespace SteamApp.WebAPI.MessageBrokers.Handlers.Wishlist;

/// <summary>
/// Wishlist Check Message Publisher/Handler
/// </summary>
/// <param name="logger"></param>
/// <param name="options"></param>
/// <param name="cache"></param>
/// <param name="wishlistService"></param>
/// <param name="messagePublisher"></param>
public sealed class WishlistCheckMessageHandler(
    ILogger<WishlistCheckMessageHandler> logger,
    IOptions<RabbitMqOptions> options,
    IDistributedCache cache,
    IWishlistService wishlistService,
    IMessagePublisher messagePublisher)
{
    private readonly RabbitMqOptions _options = options.Value;

    public async Task HandleAsync(
        WishlistCheckRequested message,
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
            await cache.RemoveAsync(queuedCacheKey, cancellationToken);
            logger.LogInformation(
                "Wishlist check message {CorrelationId} skipped because wishlist item {WishlistId} is already cached.",
                message.CorrelationId,
                message.WishlistId);
            return;
        }

        var result = await wishlistService.CheckWishlistItem(message.WishlistId);

        if (!result.IsPriceReached)
        {
            await cache.RemoveAsync(queuedCacheKey, cancellationToken);
            logger.LogInformation(
                "Wishlist check message {CorrelationId} completed for wishlist item {WishlistId}; price has not been reached.",
                message.CorrelationId,
                message.WishlistId);
            return;
        }

        await messagePublisher.PublishAsync(
            _options.WishlistNotificationQueueName,
            new WishlistNotificationRequested(
                message.WishlistId,
                message.WishlistName,
                message.Email,
                result.GameName,
                result.CurrentPrice,
                DateTime.UtcNow,
                message.CorrelationId),
            cancellationToken);

        logger.LogInformation(
            "Wishlist check message {CorrelationId} queued a notification for wishlist item {WishlistId}.",
            message.CorrelationId,
            message.WishlistId);
    }
}
