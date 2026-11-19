namespace SteamApp.Application.Abstractions.Messaging;

public interface IMessagePublisher
{
    Task PublishAsync<TMessage>(
        string queueName,
        TMessage message,
        CancellationToken cancellationToken);
}
