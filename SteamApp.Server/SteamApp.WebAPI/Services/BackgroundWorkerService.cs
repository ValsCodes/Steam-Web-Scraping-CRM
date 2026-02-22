using Microsoft.Extensions.Options;
using SteamApp.Infrastructure;
using SteamApp.Domain.ValueObjects;

namespace SteamApp.WebAPI.Services
{
    public sealed class BackgroundWorkerService<TJob>(ILogger<BackgroundWorkerService<TJob>> logger, IServiceScopeFactory scopeFactory, IOptionsMonitor<WorkerOptions> options) : BackgroundService where TJob : class, IJobService
    {
        private readonly string _name = typeof(TJob).Name;
        private PeriodicTimer? _timer;
        private readonly SemaphoreSlim _gate = new(1, 1);

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            var opts = options.Get(_name);
            if (!opts.Enabled)
            {
                logger.LogInformation("{Worker} disabled via config", _name);
                return;
            }

            _timer = new PeriodicTimer(opts.Interval);
            logger.LogInformation("{Worker} started with interval {Interval}", opts.DisplayName ?? _name, opts.Interval);

            try
            {
                while (await _timer.WaitForNextTickAsync(stoppingToken))
                {
                    // prevent overlapping executions
                    if (!await _gate.WaitAsync(0, stoppingToken))
                    {
                        logger.LogWarning("{Worker} previous tick still running; skipping", _name);
                        continue;
                    }

                    _ = TickAsync(stoppingToken).ContinueWith(_ => _gate.Release());
                }
            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
            {
                // graceful shutdown
            }
            finally
            {
                logger.LogInformation("{Worker} stopping", _name);
            }
        }

        private async Task TickAsync(CancellationToken ct)
        {
            try
            {
                using var scope = scopeFactory.CreateScope();
                var job = scope.ServiceProvider.GetRequiredService<TJob>();
                await job.RunAsync(ct);
            }
            catch (OperationCanceledException) when (ct.IsCancellationRequested)
            {
                // normal during shutdown
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "{Worker} tick failed", _name);
            }
        }
    }
}
