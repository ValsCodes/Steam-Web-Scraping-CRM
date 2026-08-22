using SteamApp.Application.DTOs.ManualCheck;

namespace SteamApp.WebAPI.Jobs;

internal sealed class ManualCheckQueueControl : IDisposable
{
    private readonly CancellationTokenSource cancellation = new();
    private readonly CancellationTokenSource pause = new();

    public ManualCheckQueueControl(long runId)
    {
        RunId = runId;
        WorkItemId = Guid.NewGuid();
    }

    public long RunId { get; }
    public Guid WorkItemId { get; }
    public bool IsPauseRequested => pause.IsCancellationRequested;

    public ManualCheckQueueItem CreateWorkItem()
    {
        return new ManualCheckQueueItem(
            RunId,
            WorkItemId,
            cancellation.Token,
            pause.Token);
    }

    public bool TryPause()
    {
        return TryCancelSource(pause);
    }

    public bool TryCancel()
    {
        return TryCancelSource(cancellation);
    }

    public void Dispose()
    {
        cancellation.Dispose();
        pause.Dispose();
    }

    private static bool TryCancelSource(CancellationTokenSource source)
    {
        try
        {
            source.Cancel();
            return true;
        }
        catch (ObjectDisposedException)
        {
            return false;
        }
    }
}
