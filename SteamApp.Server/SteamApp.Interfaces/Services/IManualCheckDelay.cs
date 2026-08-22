namespace SteamApp.Interfaces.Services;

public interface IManualCheckDelay
{
    Task DelayAsync(TimeSpan delay, CancellationToken cancellationToken);
}
