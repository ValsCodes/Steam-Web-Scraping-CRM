using System.Diagnostics;
using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Options;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using SteamApp.WebAPI.MessageBrokers.Handlers.Wishlist;
using SteamApp.WebAPI.MessageBrokers.Messages.Wishlist;
using SteamApp.WebAPI.MessageBrokers.Providers.RabbitMq.Connection;
using SteamApp.WebAPI.MessageBrokers.Providers.RabbitMq.Options;
using SteamApp.WebAPI.Observability;

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
            const string operation = "rabbitmq.wishlist-notification.process";
            var startedAt = Stopwatch.GetTimestamp();
            using var activity = SteamAppTelemetry.StartOperation(operation);
            var outcome = SteamAppTelemetry.SuccessOutcome;
            try
            {
                var message = DeserializeMessage(eventArgs.Body.ToArray());
                SteamAppTelemetry.RecordQueueDelay(
                    _options.WishlistNotificationQueueName,
                    message.RequestedAtUtc,
                    DateTime.UtcNow);

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
                outcome = SteamAppTelemetry.CancelledOutcome;
            }
            catch (Exception exception)
            {
                outcome = SteamAppTelemetry.ErrorOutcome;
                SteamAppTelemetry.MarkError(activity);
                logger.LogError(exception, "Wishlist notification message processing failed.");

                await channel.BasicNackAsync(
                    deliveryTag: eventArgs.DeliveryTag,
                    multiple: false,
                    requeue: false,
                    cancellationToken: stoppingToken);
            }
            finally
            {
                SteamAppTelemetry.CompleteOperation(
                    activity,
                    operation,
                    outcome,
                    Stopwatch.GetElapsedTime(startedAt));
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
