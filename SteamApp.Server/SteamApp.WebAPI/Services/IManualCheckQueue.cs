using System.Collections.Concurrent;
using System.Threading.Channels;

namespace SteamApp.WebAPI.Services;

public interface IManualCheckQueue
{
    ValueTask EnqueueAsync(long runId, CancellationToken cancellationToken = default);
    IAsyncEnumerable<ManualCheckQueueItem> ReadAllAsync(CancellationToken cancellationToken);
    bool TryCancel(long runId);
    void Complete(long runId);
}

public readonly record struct ManualCheckQueueItem(long RunId, CancellationToken CancellationToken);

public sealed class ManualCheckQueue : IManualCheckQueue
{
    private readonly Channel<long> channel = Channel.CreateUnbounded<long>(
        new UnboundedChannelOptions
        {
            SingleReader = true,
            SingleWriter = false
        });
    private readonly ConcurrentDictionary<long, CancellationTokenSource> cancellations = new();

    public async ValueTask EnqueueAsync(long runId, CancellationToken cancellationToken = default)
    {
        var runCancellation = new CancellationTokenSource();
        if (!cancellations.TryAdd(runId, runCancellation))
        {
            runCancellation.Dispose();
            throw new InvalidOperationException($"Manual check run {runId} is already queued.");
        }

        try
        {
            await channel.Writer.WriteAsync(runId, cancellationToken);
        }
        catch
        {
            Complete(runId);
            throw;
        }
    }

    public async IAsyncEnumerable<ManualCheckQueueItem> ReadAllAsync(
        [System.Runtime.CompilerServices.EnumeratorCancellation] CancellationToken cancellationToken)
    {
        await foreach (var runId in channel.Reader.ReadAllAsync(cancellationToken))
        {
            if (cancellations.TryGetValue(runId, out var runCancellation))
            {
                yield return new ManualCheckQueueItem(runId, runCancellation.Token);
            }
        }
    }

    public bool TryCancel(long runId)
    {
        if (!cancellations.TryGetValue(runId, out var cancellation))
        {
            return false;
        }

        try
        {
            cancellation.Cancel();
            return true;
        }
        catch (ObjectDisposedException)
        {
            return false;
        }
    }

    public void Complete(long runId)
    {
        if (cancellations.TryRemove(runId, out var cancellation))
        {
            cancellation.Dispose();
        }
    }
}
