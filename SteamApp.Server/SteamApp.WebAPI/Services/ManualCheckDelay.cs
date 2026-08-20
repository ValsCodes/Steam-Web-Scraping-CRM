namespace SteamApp.WebAPI.Services;

public sealed class ManualCheckDelay : IManualCheckDelay
{
    public Task DelayAsync(TimeSpan delay, CancellationToken cancellationToken)
    {
        return Task.Delay(delay, cancellationToken);
    }
}
