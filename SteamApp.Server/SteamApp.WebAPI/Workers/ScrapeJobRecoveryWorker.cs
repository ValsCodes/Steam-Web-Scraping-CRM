using Microsoft.Extensions.Options;
using SteamApp.Application.Abstractions.Persistence;
using SteamApp.Infrastructure.Messaging.Handlers.Scraping;
using SteamApp.Infrastructure.Messaging.RabbitMq.Options;

namespace SteamApp.WebAPI.Workers;

public sealed class ScrapeJobRecoveryWorker(
    IOptions<RabbitMqOptions> options,
    IServiceScopeFactory scopeFactory,
    ILogger<ScrapeJobRecoveryWorker> logger) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        using var timer = new PeriodicTimer(TimeSpan.FromSeconds(10));
        while (await timer.WaitForNextTickAsync(stoppingToken))
        {
            try
            {
                await using var scope = scopeFactory.CreateAsyncScope();
                var data = scope.ServiceProvider.GetRequiredService<IScrapeHistoryDataService>();
                var handler = scope.ServiceProvider.GetRequiredService<ScrapeRequestedMessageHandler>();
                var now = DateTime.UtcNow;
                var queuedBefore = options.Value.Enabled ? now.AddSeconds(-20) : now;
                foreach (var id in await data.GetRecoverableJobIdsAsync(queuedBefore, now, stoppingToken))
                {
                    await handler.RecoverAsync(id, stoppingToken);
                }
            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
            {
                return;
            }
            catch (Exception exception)
            {
                logger.LogError(exception, "Failed to recover pending scrape jobs.");
            }
        }
    }
}
