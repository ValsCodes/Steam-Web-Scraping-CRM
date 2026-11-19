using SteamApp.Application.DTOs.ManualCheck;

namespace SteamApp.WebAPI.Workers;

internal sealed class ManualCheckQueueControl(long runId) : IDisposable
{
    private readonly CancellationTokenSource cancellation = new();
    private readonly CancellationTokenSource pause = new();
    private readonly object gate = new();
    private int activeSignals;
    private bool disposalRequested;

    public long RunId { get; } = runId;
    public Guid WorkItemId { get; } = Guid.NewGuid();
    public bool IsPauseRequested
    {
        get
        {
            lock (gate)
            {
                return !disposalRequested && pause.IsCancellationRequested;
            }
        }
    }

    public ManualCheckQueueItem CreateWorkItem() =>
        new(RunId, WorkItemId, cancellation.Token, pause.Token);

    public bool TryPause() => TryCancelSource(pause);

    public bool TryCancel() => TryCancelSource(cancellation);

    public void Dispose()
    {
        lock (gate)
        {
            if (disposalRequested)
            {
                return;
            }

            disposalRequested = true;
            if (activeSignals == 0)
            {
                DisposeSources();
            }
        }
    }

    private bool TryCancelSource(CancellationTokenSource source)
    {
        lock (gate)
        {
            if (disposalRequested)
            {
                return false;
            }

            activeSignals++;
        }

        try
        {
            source.Cancel();
            return true;
        }
        finally
        {
            lock (gate)
            {
                activeSignals--;
                if (disposalRequested && activeSignals == 0)
                {
                    DisposeSources();
                }
            }
        }
    }

    private void DisposeSources()
    {
        cancellation.Dispose();
        pause.Dispose();
    }
}
