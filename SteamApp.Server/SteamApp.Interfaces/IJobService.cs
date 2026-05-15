namespace SteamApp.Interfaces
{
    public interface IJobService
    {
        Task RunAsync(CancellationToken ct);
    }
}
