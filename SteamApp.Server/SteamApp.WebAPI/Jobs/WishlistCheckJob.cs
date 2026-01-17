using SteamApp.Infrastructure;
using SteamApp.Infrastructure.Services;
using SteamApp.WebApiClient;

namespace SteamApp.WebAPI.Jobs;

public class WishlistCheckJob(ILogger<WishlistCheckJob> log, SteamApiClient apiClient, IEmailService emailSerivce) : IJobService
{
    public async Task RunAsync(CancellationToken ct)
    {
        // purge old data, compact blobs, etc.
        var wishList = await apiClient.Games.GetAllAsync(ct);

        await Task.Delay(400, ct);
        foreach (var item in wishList)
        {

            //var emailMessage = new EmailMessage(To: "ivailo1224@gmail.com", 
            //    Subject: "Welcome", 
            //    HtmlBody: "<h1>Welcome</h1><p>Your account is active.</p>", 
            //    PlainTextBody: "Welcome. Your account is active.");

            //await emailSerivce.SendAsync(emailMessage, ct);

            log.LogInformation($"WishlistCheckJob tick at {DateTime.UtcNow}: whishlist item {item.Name}");
        }
    }
}
