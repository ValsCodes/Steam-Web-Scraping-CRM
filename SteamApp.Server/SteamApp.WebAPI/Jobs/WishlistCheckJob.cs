using Microsoft.Extensions.Caching.Memory;
using SteamApp.Application.Caching;
using SteamApp.Application.Services;
using SteamApp.Interfaces;
using SteamApp.Interfaces.Services;
using SteamApp.WebAPI.Services;

namespace SteamApp.WebAPI.Jobs;

public class WishlistCheckJob(
    ILogger<WishlistCheckJob> log, 
    IEmailService emailService,
    IMemoryCache cache, 
    IWishlistService wishlistService,
    IWishlistNotificationRecipientService recipientService) : IJobService
{
    public async Task RunAsync(CancellationToken ct)
    {
        var recipients = await recipientService.GetActiveRecipientsAsync(ct);

        await Task.Delay(400, ct);

        foreach (var recipient in recipients)
        {
            try
            {
                var cacheKey = string.Format(CacheKeys.WishListBackgroundJob, recipient.WishlistId);

                if (cache.TryGetValue(cacheKey, out var cached))
                {
                    continue;
                }

                var result = await wishlistService.CheckWishlistItem(recipient.WishlistId);

                if (result != null && result.IsPriceReached)
                {
                    cache.Set(cacheKey, result, TimeSpan.FromHours(12));

                    await emailService.SendAsync(new EmailMessage(
                        To: recipient.Email,
                        Subject: $"Wishlist item {result.GameName} Price has been reached!",
                        Body: $"{result.GameName} is currently at {result.CurrentPrice} EUR"), ct);

                    log.LogInformation(
                        "WishlistCheckJob tick at {Timestamp}: wishlist item {WishlistId} has reached a price point. Email sent to {Email}.",
                        DateTime.UtcNow,
                        recipient.WishlistId,
                        recipient.Email);
                }

                log.LogInformation(
                    "WishlistCheckJob tick at {Timestamp}: wishlist item {WishlistName}",
                    DateTime.UtcNow,
                    recipient.WishlistName);
            }
            catch (Exception ex)
            {
                log.LogError(
                    ex,
                    "WishlistCheckJob had tick at {Timestamp}: there was an error for wishlist item {WishlistId}.",
                    DateTime.UtcNow,
                    recipient.WishlistId);
            }

            // Wait 15 secs between each item check to avoid hitting API limits and to be more polite to the API server.
            await Task.Delay(15000, ct);
        }
    }
}
