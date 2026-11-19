namespace SteamApp.Application.Abstractions.Services;

public interface IManualCheckDelay
{
    Task DelayAsync(TimeSpan delay, CancellationToken cancellationToken);
}
