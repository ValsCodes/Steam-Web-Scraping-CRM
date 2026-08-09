using SteamApp.WebAPI.MessageBrokers.Abstractions;
using SteamApp.WebAPI.MessageBrokers.Handlers.Scraping;
using SteamApp.WebAPI.MessageBrokers.Handlers.Wishlist;
using SteamApp.WebAPI.MessageBrokers.Providers.RabbitMq.Connection;
using SteamApp.WebAPI.MessageBrokers.Providers.RabbitMq.Consumers;
using SteamApp.WebAPI.MessageBrokers.Providers.RabbitMq.Options;
using SteamApp.WebAPI.MessageBrokers.Providers.RabbitMq.Publishing;

namespace SteamApp.WebAPI.MessageBrokers.Providers.RabbitMq.DependencyInjection;

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
