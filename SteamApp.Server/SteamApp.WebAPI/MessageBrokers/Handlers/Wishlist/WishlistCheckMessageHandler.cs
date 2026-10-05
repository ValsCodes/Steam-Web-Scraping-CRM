using System.Diagnostics;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Options;
using SteamApp.Application.Caching;
using SteamApp.Interfaces.Services;
using SteamApp.WebAPI.Caching;
using SteamApp.WebAPI.MessageBrokers.Abstractions;
using SteamApp.WebAPI.MessageBrokers.Messages.Wishlist;
using SteamApp.WebAPI.MessageBrokers.Providers.RabbitMq.Options;
using SteamApp.WebAPI.Observability;
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
            SteamAppTelemetry.MarkSkipped(Activity.Current);
            await cache.RemoveAsync(queuedCacheKey, cancellationToken);
            logger.LogInformation(
                "Wishlist check message {CorrelationId} skipped because the item is already cached.",
                message.CorrelationId);
            return;
        }

        var recipient = await recipientService.GetActiveRecipientAsync(
            message.WishlistId,
            cancellationToken);
        if (recipient is null)
        {
            SteamAppTelemetry.MarkSkipped(Activity.Current);
            await cache.RemoveAsync(queuedCacheKey, cancellationToken);
            logger.LogInformation(
                "Wishlist check message {CorrelationId} skipped because the item is no longer active.",
                message.CorrelationId);
            return;
        }

        var executionResult = await checkExecution.ExecuteScheduledAsync(
            message.WishlistId,
            message.RequestedAtUtc,
            message.CorrelationId,
            cancellationToken);

        if (executionResult.IsFailure)
        {
            SteamAppTelemetry.MarkSkipped(Activity.Current);
            await cache.RemoveAsync(queuedCacheKey, cancellationToken);
            logger.LogWarning(
                "Wishlist check message {CorrelationId} skipped: {ErrorCode}.",
                message.CorrelationId,
                executionResult.Error!.Code);
            return;
        }

        var outcome = executionResult.Value!;
        if (outcome.CheckError is not null)
        {
            SteamAppTelemetry.MarkError(Activity.Current);
            await cache.RemoveAsync(queuedCacheKey, cancellationToken);
            logger.LogWarning(
                "Wishlist check message {CorrelationId} failed: {ErrorCode}.",
                message.CorrelationId,
                outcome.CheckError.Code);
            return;
        }

        var result = outcome.Trace;

        if (result.IsPriceReached != true)
        {
            await cache.RemoveAsync(queuedCacheKey, cancellationToken);
            logger.LogInformation(
                "Wishlist check message {CorrelationId} completed; price has not been reached.",
                message.CorrelationId);
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
            "Wishlist check message {CorrelationId} queued a notification.",
            message.CorrelationId);
    }
}
