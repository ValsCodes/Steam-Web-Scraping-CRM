namespace SteamApp.IntegrationTests.Support;

public sealed record PublishedMessage(string QueueName, object Message);
