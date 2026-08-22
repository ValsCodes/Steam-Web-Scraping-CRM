using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Options;
using SteamApp.Application.Caching;
using SteamApp.Interfaces;
using SteamApp.Interfaces.Services;
using SteamApp.WebAPI.Caching;
using SteamApp.WebAPI.MessageBrokers.Abstractions;
using SteamApp.WebAPI.MessageBrokers.Messages.Wishlist;
using SteamApp.WebAPI.MessageBrokers.Providers.RabbitMq.Options;

namespace SteamApp.WebAPI.Jobs;

public class WishlistCheckJob(
    ILogger<WishlistCheckJob> log,
    IOptions<RabbitMqOptions> options,
    IDistributedCache cache,
    IMessagePublisher messagePublisher,
    IWishlistNotificationRecipientService recipientService) : IJobService
{
    private static readonly TimeSpan QueuedMarkerTtl = TimeSpan.FromMinutes(5);
    private readonly RabbitMqOptions _options = options.Value;

    public async Task RunAsync(CancellationToken ct)
    {
        var recipients = await recipientService.GetActiveRecipientsAsync(ct);

        foreach (var recipient in recipients)
        {
            try
            {
                var notificationCacheKey = string.Format(
                    CacheKeys.WishListBackgroundJob,
                    recipient.WishlistId);
                var queuedCacheKey = string.Format(
                    CacheKeys.WishListBackgroundJobQueued,
                    recipient.WishlistId);

                if (await cache.ExistsAsync(notificationCacheKey, ct) ||
                    await cache.ExistsAsync(queuedCacheKey, ct))
                {
                    continue;
                }

                var message = new WishlistCheckRequested(
                    recipient.WishlistId,
                    recipient.WishlistName,
                    recipient.Email,
                    DateTime.UtcNow,
                    Guid.NewGuid().ToString("N"));

                await messagePublisher.PublishAsync(
                    _options.WishlistCheckQueueName,
                    message,
                    ct);

                await cache.SetMarkerAsync(queuedCacheKey, QueuedMarkerTtl, ct);

                log.LogInformation(
                    "WishlistCheckJob queued wishlist item {WishlistId} for {Email}.",
                    recipient.WishlistId,
                    recipient.Email);
            }
            catch (OperationCanceledException) when (ct.IsCancellationRequested)
            {
                throw;
            }
            catch (Exception ex)
            {
                log.LogError(
                    ex,
                    "WishlistCheckJob failed to queue wishlist item {WishlistId}.",
                    recipient.WishlistId);
            }
        }
    }
}
