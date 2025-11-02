using SteamApp.Infrastructure;

namespace SteamApp.WebAPI.Jobs
{
    public sealed class EmailJob(ILogger<EmailJob> log) : IJob
    {
        public async Task RunAsync(CancellationToken ct)
        {
            // send pending emails
            await Task.Delay(250, ct);
            log.LogInformation("EmailJob tick at {Utc}", DateTime.UtcNow);
        }
    }
}
