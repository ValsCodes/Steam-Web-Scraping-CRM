using SteamApp.WebAPI.Jobs;

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
}
