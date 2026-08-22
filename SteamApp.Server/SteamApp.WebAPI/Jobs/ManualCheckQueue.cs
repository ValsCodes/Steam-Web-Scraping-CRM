using System.Collections.Concurrent;
using System.Threading.Channels;
using SteamApp.Application.DTOs.ManualCheck;
using SteamApp.Interfaces.Services;

namespace SteamApp.WebAPI.Jobs;

public sealed class ManualCheckQueue : IManualCheckQueue
{
    private readonly Channel<ManualCheckQueueItem> channel = Channel.CreateUnbounded<ManualCheckQueueItem>(
        new UnboundedChannelOptions
        {
            SingleReader = true,
            SingleWriter = false
        });
    private readonly ConcurrentDictionary<Guid, ManualCheckQueueControl> controls = new();
    private readonly ConcurrentDictionary<long, Guid> currentWorkItems = new();

    public async ValueTask EnqueueAsync(long runId, CancellationToken cancellationToken = default)
    {
        var control = new ManualCheckQueueControl(runId);
        if (!controls.TryAdd(control.WorkItemId, control))
        {
            control.Dispose();
            throw new InvalidOperationException($"Unable to register manual check run {runId}.");
        }

        try
        {
            SetCurrentWorkItem(control);
            await channel.Writer.WriteAsync(control.CreateWorkItem(), cancellationToken);
        }
        catch
        {
            Complete(runId, control.WorkItemId);
            throw;
        }
    }

    public async IAsyncEnumerable<ManualCheckQueueItem> ReadAllAsync(
        [System.Runtime.CompilerServices.EnumeratorCancellation] CancellationToken cancellationToken)
    {
        await foreach (var workItem in channel.Reader.ReadAllAsync(cancellationToken))
        {
            if (IsCurrent(workItem.RunId, workItem.WorkItemId))
            {
                yield return workItem;
            }
            else
            {
                Complete(workItem.RunId, workItem.WorkItemId);
            }
        }
    }

    public bool TryPause(long runId)
    {
        return TryGetCurrentControl(runId, out var control) && control.TryPause();
    }

    public bool TryCancel(long runId)
    {
        return TryGetCurrentControl(runId, out var control) && control.TryCancel();
    }

    public void Complete(long runId, Guid workItemId)
    {
        RemoveCurrent(runId, workItemId);
        if (controls.TryRemove(workItemId, out var control))
        {
            control.Dispose();
        }
    }

    private void SetCurrentWorkItem(ManualCheckQueueControl control)
    {
        while (true)
        {
            if (!currentWorkItems.TryGetValue(control.RunId, out var existingWorkItemId))
            {
                if (currentWorkItems.TryAdd(control.RunId, control.WorkItemId))
                {
                    return;
                }

                continue;
            }

            if (!controls.TryGetValue(existingWorkItemId, out var existingControl) ||
                !existingControl.IsPauseRequested)
            {
                throw new InvalidOperationException($"Manual check run {control.RunId} is already queued.");
            }

            if (currentWorkItems.TryUpdate(control.RunId, control.WorkItemId, existingWorkItemId))
            {
                return;
            }
        }
    }

    private bool TryGetCurrentControl(long runId, out ManualCheckQueueControl control)
    {
        if (currentWorkItems.TryGetValue(runId, out var workItemId) &&
            controls.TryGetValue(workItemId, out var currentControl))
        {
            control = currentControl;
            return true;
        }

        control = null!;
        return false;
    }

    private bool IsCurrent(long runId, Guid workItemId)
    {
        return currentWorkItems.TryGetValue(runId, out var currentWorkItemId) &&
               currentWorkItemId == workItemId;
    }

    private void RemoveCurrent(long runId, Guid workItemId)
    {
        var pair = new KeyValuePair<long, Guid>(runId, workItemId);
        ((ICollection<KeyValuePair<long, Guid>>)currentWorkItems).Remove(pair);
    }
}
