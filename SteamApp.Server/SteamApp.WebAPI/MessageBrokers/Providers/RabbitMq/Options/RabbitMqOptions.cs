namespace SteamApp.WebAPI.MessageBrokers.Providers.RabbitMq.Options;

public sealed class RabbitMqOptions
{
    public bool Enabled { get; init; }
    public string HostName { get; init; } = "localhost";
    public int Port { get; init; } = 5672;
    public string UserName { get; init; } = "guest";
    public string Password { get; init; } = "guest";
    public string VirtualHost { get; init; } = "/";
    public string WishlistCheckQueueName { get; init; } = "wishlist.check.requests";
    public string WishlistNotificationQueueName { get; init; } = "wishlist.notification.requests";
    public string ScrapeRequestQueueName { get; init; } = "scrape.requests";
    public TimeSpan WishlistCheckDelay { get; init; } = TimeSpan.FromSeconds(15);
}
