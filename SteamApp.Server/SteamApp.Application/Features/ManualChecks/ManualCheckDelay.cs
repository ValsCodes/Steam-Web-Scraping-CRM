using SteamApp.Application.Abstractions.Services;

namespace SteamApp.Application.Features.ManualChecks;

public sealed class ManualCheckDelay : IManualCheckDelay
{
    public Task DelayAsync(TimeSpan delay, CancellationToken cancellationToken)
    {
        return Task.Delay(delay, cancellationToken);
    }
}
