using SteamApp.Infrastructure;

namespace SteamApp.WebAPI.Jobs;

public class WishlistCheckJob(ILogger<WishlistCheckJob> log, SteamApiClient apiClient) : IJobService
{
    public async Task RunAsync(CancellationToken ct)
    {
        // purge old data, compact blobs, etc.
        var wishList = await apiClient.GetGamesAsync(ct);

        await Task.Delay(400, ct);
        foreach (var item in wishList)
        {
            log.LogInformation($"WishlistCheckJob tick at {DateTime.UtcNow}: whishlist item {item.Name}");
        }
    }
}
