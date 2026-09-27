using SteamApp.Interfaces.Services;

namespace SteamApp.WebAPI.Jobs;

public sealed class AutomaticQueueWorker(
    IServiceScopeFactory scopeFactory,
    ILogger<AutomaticQueueWorker> logger) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!await RecoverAsync(stoppingToken))
        {
            await Task.Delay(TimeSpan.FromSeconds(5), stoppingToken);
        }
        using var timer = new PeriodicTimer(TimeSpan.FromSeconds(1));
        while (await timer.WaitForNextTickAsync(stoppingToken))
        {
            try
            {
                using var scope = scopeFactory.CreateScope();
                var service = scope.ServiceProvider.GetRequiredService<IAutomaticQueueDataService>();
                await service.ProcessActiveRunsAsync(stoppingToken);
            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
            {
                break;
            }
            catch (Exception exception)
            {
                logger.LogError(exception, "Automatic queue worker iteration failed.");
            }
        }
    }

    private async Task<bool> RecoverAsync(CancellationToken cancellationToken)
    {
        try
        {
            using var scope = scopeFactory.CreateScope();
            var service = scope.ServiceProvider.GetRequiredService<IAutomaticQueueDataService>();
            await service.RecoverInterruptedRunsAsync(cancellationToken);
            return true;
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            return true;
        }
        catch (Exception exception)
        {
            logger.LogError(exception, "Automatic queue recovery failed during startup.");
            return false;
        }
    }
}
