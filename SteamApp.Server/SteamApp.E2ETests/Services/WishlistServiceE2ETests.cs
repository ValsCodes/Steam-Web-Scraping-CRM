using Microsoft.Extensions.DependencyInjection;
using SteamApp.IntegrationTests.Support;
using SteamApp.WebAPI.Jobs;
using SteamApp.WebAPI.MessageBrokers.Handlers.Wishlist;
using SteamApp.WebAPI.MessageBrokers.Messages.Wishlist;

namespace SteamApp.E2ETests.Services;

[TestFixture]
public sealed class WishlistServiceE2ETests
{
    [Test]
    public async Task BackgroundWishlistWorkflowSendsEmailOnceAndUsesCacheOnSecondRun()
    {
        using var factory = new SteamAppFactory();
        await factory.ResetDatabaseAsync();

        using var firstScope = factory.Services.CreateScope();
        var firstJob = firstScope.ServiceProvider.GetRequiredService<WishlistCheckJob>();
        await firstJob.RunAsync(CancellationToken.None);

        var checkMessage = factory.MessagePublisher
            .GetMessages<WishlistCheckRequested>()
            .Single();
        var checkHandler = firstScope.ServiceProvider.GetRequiredService<WishlistCheckMessageHandler>();
        await checkHandler.HandleAsync(checkMessage, CancellationToken.None);

        var notificationMessage = factory.MessagePublisher
            .GetMessages<WishlistNotificationRequested>()
            .Single();
        var notificationHandler = firstScope.ServiceProvider.GetRequiredService<WishlistNotificationMessageHandler>();
        await notificationHandler.HandleAsync(notificationMessage, CancellationToken.None);

        using var secondScope = factory.Services.CreateScope();
        var secondJob = secondScope.ServiceProvider.GetRequiredService<WishlistCheckJob>();

        await secondJob.RunAsync(CancellationToken.None);

        Assert.Multiple(() =>
        {
            Assert.That(factory.EmailService.Messages, Has.Count.EqualTo(1));
            Assert.That(factory.EmailService.Messages.Single().To, Is.EqualTo(IntegrationSeed.UserEmail));
            Assert.That(factory.EmailService.Messages.Single().Subject, Does.Contain("Active Game"));
            Assert.That(factory.WishlistService.CheckCalls, Is.EqualTo(1));
            Assert.That(factory.MessagePublisher.GetMessages<WishlistCheckRequested>(), Has.Count.EqualTo(1));
        });
    }
}
