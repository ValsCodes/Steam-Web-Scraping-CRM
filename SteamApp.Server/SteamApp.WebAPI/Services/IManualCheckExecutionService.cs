namespace SteamApp.WebAPI.Services;

public interface IManualCheckExecutionService
{
    Task ExecuteAsync(long runId, CancellationToken cancellationToken);
}
