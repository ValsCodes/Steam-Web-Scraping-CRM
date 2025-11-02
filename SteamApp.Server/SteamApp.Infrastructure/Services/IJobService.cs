namespace SteamApp.Infrastructure
{
    public interface IJobService
    {
        Task RunAsync(CancellationToken ct);
    }
}
