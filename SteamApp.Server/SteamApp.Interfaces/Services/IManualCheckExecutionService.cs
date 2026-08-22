namespace SteamApp.Interfaces.Services;

public interface IManualCheckExecutionService
{
    Task ExecuteAsync(
        long runId,
        CancellationToken cancellationToken,
        CancellationToken pauseToken);
}
