namespace SteamApp.Application.Abstractions
{
    public interface IJobService
    {
        Task RunAsync(CancellationToken ct);
    }
}
