using RabbitMQ.Client;
using SteamApp.WebAPI.MessageBrokers.Abstractions;
using SteamApp.WebAPI.MessageBrokers.Providers.RabbitMq.Connection;

namespace SteamApp.WebAPI.MessageBrokers.Providers.RabbitMq.Publishing;

public sealed class RabbitMqPublisher(
    RabbitMqConnection rabbitMqConnection) : IMessagePublisher
{
    public async Task PublishAsync<TMessage>(
        string queueName,
        TMessage message,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(queueName))
        {
            throw new ArgumentException("Queue name is required.", nameof(queueName));
        }

        IConnection connection = await rabbitMqConnection.GetConnectionAsync(cancellationToken);

        await using IChannel channel = await connection.CreateChannelAsync(
            cancellationToken: cancellationToken);

        await channel.QueueDeclareAsync(
            queue: queueName,
            durable: true,
            exclusive: false,
            autoDelete: false,
            arguments: null,
            cancellationToken: cancellationToken);

        byte[] body = System.Text.Json.JsonSerializer.SerializeToUtf8Bytes(message);

        BasicProperties properties = new()
        {
            ContentType = "application/json",
            Persistent = true,
            MessageId = Guid.NewGuid().ToString("N")
        };

        await channel.BasicPublishAsync(
            exchange: string.Empty,
            routingKey: queueName,
            mandatory: true,
            basicProperties: properties,
            body: body,
            cancellationToken: cancellationToken);
    }
}
