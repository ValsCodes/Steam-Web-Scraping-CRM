using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using SteamApp.Application.Abstractions.Messaging;
using SteamApp.Infrastructure.Messaging.Handlers.Scraping;
using SteamApp.Infrastructure.Messaging.Handlers.Wishlist;
using SteamApp.Infrastructure.Messaging.RabbitMq.Connection;
using SteamApp.Infrastructure.Messaging.RabbitMq.Consumers;
using SteamApp.Infrastructure.Messaging.RabbitMq.Options;
using SteamApp.Infrastructure.Messaging.RabbitMq.Publishing;

namespace SteamApp.Infrastructure.Messaging.RabbitMq.DependencyInjection;

public static class RabbitMqServiceCollectionExtensions
{
    public static IServiceCollection AddRabbitMqMessageBroker(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        ArgumentNullException.ThrowIfNull(services);
        ArgumentNullException.ThrowIfNull(configuration);

        ValidateWishlistBrokerConfiguration(configuration);

        var rabbitMqOptions = configuration.GetSection("RabbitMq").Get<RabbitMqOptions>() ?? new RabbitMqOptions();

        services.Configure<RabbitMqOptions>(configuration.GetSection("RabbitMq"));
        services.AddSingleton<RabbitMqConnection>();
        services.AddSingleton<IMessagePublisher, RabbitMqPublisher>();
        services.AddScoped<ScrapeRequestedMessageHandler>();
        services.AddScoped<WishlistCheckMessageHandler>();
        services.AddScoped<WishlistNotificationMessageHandler>();

        if (rabbitMqOptions.Enabled)
        {
            services.AddHostedService<ScrapeRequestedConsumer>();
            services.AddHostedService<WishlistCheckConsumer>();
            services.AddHostedService<WishlistNotificationConsumer>();
        }

        return services;
    }

    private static void ValidateWishlistBrokerConfiguration(IConfiguration configuration)
    {
        var wishlistWorkerEnabled = configuration.GetValue<bool>("Workers:WishlistCheck:Enabled");
        var rabbitMqEnabled = configuration.GetValue<bool>("RabbitMq:Enabled");

        if (wishlistWorkerEnabled && !rabbitMqEnabled)
        {
            throw new InvalidOperationException(
                "Workers:WishlistCheck requires RabbitMq:Enabled to be true.");
        }
    }
}
