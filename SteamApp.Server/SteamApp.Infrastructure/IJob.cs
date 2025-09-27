namespace SteamApp.Infrastructure
{
    public interface IJob
    {
        Task RunAsync(CancellationToken ct);
    }
}
