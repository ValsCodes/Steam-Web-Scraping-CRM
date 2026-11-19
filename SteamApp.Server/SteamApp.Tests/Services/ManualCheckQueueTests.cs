using SteamApp.WebAPI.Workers;

namespace SteamApp.Tests.Services;

[TestFixture]
public sealed class ManualCheckQueueTests
{
    [Test]
    public async Task TryCancelSignalsTheQueuedRunsCancellationToken()
    {
        var queue = new ManualCheckQueue();
        await queue.EnqueueAsync(42);
        await using var reader = queue.ReadAllAsync(CancellationToken.None).GetAsyncEnumerator();

        Assert.That(await reader.MoveNextAsync(), Is.True);
        Assert.That(reader.Current.RunId, Is.EqualTo(42));

        Assert.That(queue.TryCancel(42), Is.True);
        Assert.That(reader.Current.CancellationToken.IsCancellationRequested, Is.True);

        queue.Complete(42, reader.Current.WorkItemId);
        Assert.That(queue.TryCancel(42), Is.False);
    }

    [Test]
    public async Task EnqueueAsync_AfterPause_SkipsTheStaleWorkItemGeneration()
    {
        var queue = new ManualCheckQueue();
        await queue.EnqueueAsync(42);
        Assert.That(queue.TryPause(42), Is.True);

        await queue.EnqueueAsync(42);
        await using var reader = queue.ReadAllAsync(CancellationToken.None).GetAsyncEnumerator();

        Assert.That(await reader.MoveNextAsync(), Is.True);
        Assert.Multiple(() =>
        {
            Assert.That(reader.Current.RunId, Is.EqualTo(42));
            Assert.That(reader.Current.PauseToken.IsCancellationRequested, Is.False);
        });

        queue.Complete(42, reader.Current.WorkItemId);
    }

    [Test]
    public async Task Complete_DuringCancellation_DefersDisposalUntilCallbacksFinish()
    {
        var queue = new ManualCheckQueue();
        await queue.EnqueueAsync(1);
        await using var reader = queue.ReadAllAsync(CancellationToken.None).GetAsyncEnumerator();
        Assert.That(await reader.MoveNextAsync(), Is.True);

        var workItem = reader.Current;
        var entered = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
        var resume = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
        using var registration = workItem.CancellationToken.Register(() =>
        {
            entered.TrySetResult();
            resume.Task.GetAwaiter().GetResult();
            Assert.That(workItem.CancellationToken.WaitHandle.WaitOne(0), Is.True);
        });

        var cancellation = Task.Run(() => queue.TryCancel(workItem.RunId));
        try
        {
            await entered.Task.WaitAsync(TimeSpan.FromSeconds(5));
            queue.Complete(workItem.RunId, workItem.WorkItemId);
        }
        finally
        {
            resume.TrySetResult();
        }

        Assert.That(await cancellation, Is.True);
    }
}
