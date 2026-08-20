using Microsoft.Extensions.Options;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using SteamApp.WebAPI.MessageBrokers.Handlers.Wishlist;
using SteamApp.WebAPI.MessageBrokers.Messages.Wishlist;
using SteamApp.WebAPI.MessageBrokers.Providers.RabbitMq.Connection;
using SteamApp.WebAPI.MessageBrokers.Providers.RabbitMq.Options;
using System.Text;
using System.Text.Json;

namespace SteamApp.WebAPI.MessageBrokers.Providers.RabbitMq.Consumers;

public sealed class WishlistNotificationConsumer(
    RabbitMqConnection rabbitMqConnection,
    IOptions<RabbitMqOptions> options,
    IServiceScopeFactory scopeFactory,
    ILogger<WishlistNotificationConsumer> logger) : BackgroundService
{
    private static readonly JsonSerializerOptions SerializerOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

    private readonly RabbitMqOptions _options = options.Value;

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        IConnection connection = await rabbitMqConnection.GetConnectionAsync(stoppingToken);

        await using IChannel channel = await connection.CreateChannelAsync(
            cancellationToken: stoppingToken);

        await channel.QueueDeclareAsync(
            queue: _options.WishlistNotificationQueueName,
            durable: true,
            exclusive: false,
            autoDelete: false,
            arguments: null,
            cancellationToken: stoppingToken);

        await channel.BasicQosAsync(
            prefetchSize: 0,
            prefetchCount: 1,
            global: false,
            cancellationToken: stoppingToken);

        AsyncEventingBasicConsumer consumer = new(channel);

        consumer.ReceivedAsync += async (_, eventArgs) =>
        {
            try
            {
                var message = DeserializeMessage(eventArgs.Body.ToArray());

                using var scope = scopeFactory.CreateScope();
                var handler = scope.ServiceProvider.GetRequiredService<WishlistNotificationMessageHandler>();

                await handler.HandleAsync(message, stoppingToken);

                await channel.BasicAckAsync(
                    deliveryTag: eventArgs.DeliveryTag,
                    multiple: false,
                    cancellationToken: stoppingToken);
            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
            {
            }
            catch (Exception exception)
            {
                logger.LogError(exception, "Wishlist notification message processing failed.");

                await channel.BasicNackAsync(
                    deliveryTag: eventArgs.DeliveryTag,
                    multiple: false,
                    requeue: false,
                    cancellationToken: stoppingToken);
            }
        };

        await channel.BasicConsumeAsync(
            queue: _options.WishlistNotificationQueueName,
            autoAck: false,
            consumer: consumer,
            cancellationToken: stoppingToken);

        logger.LogInformation(
            "Wishlist notification RabbitMQ consumer started for queue {QueueName}.",
            _options.WishlistNotificationQueueName);

        try
        {
            await Task.Delay(Timeout.InfiniteTimeSpan, stoppingToken);
        }
        catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
        {
        }
    }

    private static WishlistNotificationRequested DeserializeMessage(byte[] body)
    {
        string json = Encoding.UTF8.GetString(body);

        return JsonSerializer.Deserialize<WishlistNotificationRequested>(json, SerializerOptions)
            ?? throw new InvalidOperationException("Wishlist notification message payload is empty.");
    }
}
