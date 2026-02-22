using SteamApp.Infrastructure;
using SteamApp.Infrastructure.Services;
using SteamApp.WebApiClient;

namespace SteamApp.WebAPI.Jobs;

public class WishlistCheckJob(ILogger<WishlistCheckJob> log, SteamApiClient apiClient, IEmailService emailSerivce) : IJobService
{
    public async Task RunAsync(CancellationToken ct)
    {
        var wishList = await apiClient.WishList.GetAllAsync(ct);

        await Task.Delay(400, ct);

        foreach (var item in wishList.Where(x => x.IsActive))
        {
            try
            {
                var result = await apiClient.Steam.CheckWishlistItem(item.Id);

                if (result != null && result.IsPriceReached)
                {
                    await emailSerivce.SendAsync(new EmailMessage(To: "ivailo1224@gmail.com",
                                     Subject: $"Wishlist item {result.GameName} Price has been reached!",
                                     Body: $"{result.GameName} is currently at {result.CurrentPrice} EUR"), ct);

                    log.LogInformation($"WishlistCheckJob tick at {DateTime.UtcNow}: whishlist item has reached a price point. Email sent.");
                }

                log.LogInformation($"WishlistCheckJob tick at {DateTime.UtcNow}: whishlist item {item.Name}");
            }
            catch (Exception ex)
            {
                log.LogError($"WishlistCheckJob had tick at {DateTime.UtcNow}: There was an Error: {ex.Message}");
            }

            // Wait 15 secs between each item check to avoid hitting API limits and to be more polite to the API server.
            await Task.Delay(15000, ct);
        }
    }
}
