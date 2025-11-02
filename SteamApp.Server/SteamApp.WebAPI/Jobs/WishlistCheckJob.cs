using SteamApp.Infrastructure;

namespace SteamApp.WebAPI.Jobs
{
    public class WishlistCheckJob(ILogger<WishlistCheckJob> log) : IJobService
    {
        public async Task RunAsync(CancellationToken ct)
        {
            // purge old data, compact blobs, etc.
            await Task.Delay(400, ct);
            log.LogInformation("WishlistCheckJob tick at {Utc}", DateTime.UtcNow);
        }
    }
}
