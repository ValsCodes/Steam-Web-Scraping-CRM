using System.Collections.Concurrent;
using SteamApp.WebAPI.MessageBrokers.Abstractions;

namespace SteamApp.IntegrationTests.Support;

public sealed class CapturingMessagePublisher : IMessagePublisher
{
    private readonly ConcurrentQueue<PublishedMessage> messages = new();

    public IReadOnlyCollection<PublishedMessage> Messages => messages.ToArray();

    public Task PublishAsync<TMessage>(
        string queueName,
        TMessage message,
        CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        messages.Enqueue(new PublishedMessage(queueName, message!));
        return Task.CompletedTask;
    }

    public IReadOnlyCollection<TMessage> GetMessages<TMessage>(string? queueName = null)
    {
        return messages
            .Where(message =>
                message.Message is TMessage &&
                (queueName is null || message.QueueName == queueName))
            .Select(message => (TMessage)message.Message)
            .ToArray();
    }

    public void Clear()
    {
        while (messages.TryDequeue(out _))
        {
        }
    }
}
