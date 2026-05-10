using Microsoft.Extensions.DependencyInjection;
using SteamApp.IntegrationTests.Support;
using SteamApp.WebAPI.Jobs;

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
        using var firstCancellation = new CancellationTokenSource(TimeSpan.FromSeconds(1));

        Assert.ThrowsAsync(
            Is.InstanceOf<OperationCanceledException>(),
            async () => await firstJob.RunAsync(firstCancellation.Token));

        using var secondScope = factory.Services.CreateScope();
        var secondJob = secondScope.ServiceProvider.GetRequiredService<WishlistCheckJob>();

        await secondJob.RunAsync(CancellationToken.None);

        Assert.Multiple(() =>
        {
            Assert.That(factory.EmailService.Messages, Has.Count.EqualTo(1));
            Assert.That(factory.EmailService.Messages.Single().Subject, Does.Contain("Active Game"));
            Assert.That(factory.WishlistService.CheckCalls, Is.EqualTo(1));
        });
    }
}
