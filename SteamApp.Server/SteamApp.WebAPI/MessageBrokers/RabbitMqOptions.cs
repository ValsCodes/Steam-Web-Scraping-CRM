namespace SteamApp.WebAPI.MessageBrokers
{
    public sealed class RabbitMqOptions
    {
        public string HostName { get; init; } = "localhost";
        public int Port { get; init; } = 5672;
        public string UserName { get; init; } = "guest";
        public string Password { get; init; } = "guest";
        public string VirtualHost { get; init; } = "/";
        public string QueueName { get; init; } = "test.messages";
    }
}
