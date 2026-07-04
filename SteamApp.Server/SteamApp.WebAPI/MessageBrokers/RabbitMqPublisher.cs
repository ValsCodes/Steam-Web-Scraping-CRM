using Microsoft.Extensions.Options;
using RabbitMQ.Client;

namespace SteamApp.WebAPI.MessageBrokers;

public sealed class RabbitMqPublisher(
RabbitMqConnection rabbitMqConnection,
    IOptions<RabbitMqOptions> options) : IMessagePublisher
{
    private readonly RabbitMqOptions _options = options.Value;

    public async Task PublishAsync(
        PublishMessageRequest message,
        CancellationToken cancellationToken)
    {
        IConnection connection = await rabbitMqConnection.GetConnectionAsync(cancellationToken);

        await using IChannel channel = await connection.CreateChannelAsync(
            cancellationToken: cancellationToken);

        await channel.QueueDeclareAsync(
            queue: _options.QueueName,
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
            routingKey: _options.QueueName,
            mandatory: true,
            basicProperties: properties,
            body: body,
            cancellationToken: cancellationToken);
    }
}
