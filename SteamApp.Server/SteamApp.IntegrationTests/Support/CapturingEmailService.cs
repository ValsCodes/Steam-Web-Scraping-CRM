using System.Collections.Concurrent;
using SteamApp.Interfaces.Services;

namespace SteamApp.IntegrationTests.Support;

public sealed class CapturingEmailService : IEmailService
{
    private readonly ConcurrentQueue<EmailMessage> messages = new();

    public IReadOnlyCollection<EmailMessage> Messages => messages.ToArray();

    public Task SendAsync(
        EmailMessage message,
        CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        messages.Enqueue(message);
        return Task.CompletedTask;
    }

    public void Clear()
    {
        while (messages.TryDequeue(out _))
        {
        }
    }
}
