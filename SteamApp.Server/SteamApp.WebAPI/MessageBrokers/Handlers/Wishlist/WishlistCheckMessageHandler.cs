using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Options;
using SteamApp.Application.Caching;
using SteamApp.Interfaces.Services;
using SteamApp.WebAPI.Caching;
using SteamApp.WebAPI.MessageBrokers.Abstractions;
using SteamApp.WebAPI.MessageBrokers.Messages.Wishlist;
using SteamApp.WebAPI.MessageBrokers.Providers.RabbitMq.Options;
using SteamApp.WebAPI.Services;

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
    IWishlistCheckExecutionService checkExecution,
    IWishlistNotificationRecipientService recipientService,
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

        var recipient = await recipientService.GetActiveRecipientAsync(
            message.WishlistId,
            cancellationToken);
        if (recipient is null)
        {
            await cache.RemoveAsync(queuedCacheKey, cancellationToken);
            logger.LogInformation(
                "Wishlist check message {CorrelationId} skipped because wishlist item {WishlistId} is no longer active.",
                message.CorrelationId,
                message.WishlistId);
            return;
        }

        var executionResult = await checkExecution.ExecuteScheduledAsync(
            message.WishlistId,
            message.RequestedAtUtc,
            message.CorrelationId,
            cancellationToken);

        if (executionResult.IsFailure)
        {
            await cache.RemoveAsync(queuedCacheKey, cancellationToken);
            logger.LogWarning(
                "Wishlist check message {CorrelationId} skipped for wishlist item {WishlistId}: {ErrorCode}.",
                message.CorrelationId,
                message.WishlistId,
                executionResult.Error!.Code);
            return;
        }

        var outcome = executionResult.Value!;
        if (outcome.CheckError is not null)
        {
            await cache.RemoveAsync(queuedCacheKey, cancellationToken);
            logger.LogWarning(
                "Wishlist check message {CorrelationId} failed for wishlist item {WishlistId}: {ErrorCode}.",
                message.CorrelationId,
                message.WishlistId,
                outcome.CheckError.Code);
            return;
        }

        var result = outcome.Trace;

        if (result.IsPriceReached != true)
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
                recipient.WishlistName,
                recipient.Email,
                result.GameName,
                result.CurrentPrice!.Value,
                DateTime.UtcNow,
                message.CorrelationId),
            cancellationToken);

        logger.LogInformation(
            "Wishlist check message {CorrelationId} queued a notification for wishlist item {WishlistId}.",
            message.CorrelationId,
            message.WishlistId);
    }
}
