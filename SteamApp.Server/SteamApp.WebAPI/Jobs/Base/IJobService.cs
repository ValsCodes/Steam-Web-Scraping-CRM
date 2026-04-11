namespace SteamApp.WebAPI.Jobs.Base
{
    public interface IJobService
    {
        Task RunAsync(CancellationToken ct);
    }
}
