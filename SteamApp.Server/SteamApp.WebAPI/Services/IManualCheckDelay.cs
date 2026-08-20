namespace SteamApp.WebAPI.Services;

public interface IManualCheckDelay
{
    Task DelayAsync(TimeSpan delay, CancellationToken cancellationToken);
}
