using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging.Abstractions;
using SteamApp.Application.Caching;
using SteamApp.Application.DTOs.WishListItem;
using SteamApp.IntegrationTests.Support;
using SteamApp.WebAPI.Jobs;

namespace SteamApp.IntegrationTests.Jobs;

[TestFixture]
public sealed class WishlistJobIntegrationTests
{
    [Test]
    public void WishlistCheckJobSendsEmailWhenActiveItemReachesPrice()
    {
        var email = new CapturingEmailService();
        using var cache = new MemoryCache(new MemoryCacheOptions());
        var wishlist = new FakeWishlistService
        {
            Items =
            [
                new WishListDto
                {
                    Id = 1,
                    Name = "Active Wish",
                    IsActive = true,
                    Price = 5
                }
            ]
        };
        var job = new WishlistCheckJob(
            NullLogger<WishlistCheckJob>.Instance,
            email,
            cache,
            wishlist);
        using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(1));

        Assert.ThrowsAsync(
            Is.InstanceOf<OperationCanceledException>(),
            async () => await job.RunAsync(cts.Token));

        Assert.Multiple(() =>
        {
            Assert.That(email.Messages, Has.Count.EqualTo(1));
            Assert.That(email.Messages.Single().Subject, Does.Contain("Active Game"));
            Assert.That(wishlist.CheckCalls, Is.EqualTo(1));
        });
    }

    [Test]
    public async Task WishlistCheckJobSkipsInactiveItems()
    {
        var email = new CapturingEmailService();
        using var cache = new MemoryCache(new MemoryCacheOptions());
        var wishlist = new FakeWishlistService
        {
            Items =
            [
                new WishListDto
                {
                    Id = 2,
                    Name = "Inactive Wish",
                    IsActive = false,
                    Price = 5
                }
            ]
        };
        var job = new WishlistCheckJob(
            NullLogger<WishlistCheckJob>.Instance,
            email,
            cache,
            wishlist);

        await job.RunAsync(CancellationToken.None);

        Assert.Multiple(() =>
        {
            Assert.That(email.Messages, Is.Empty);
            Assert.That(wishlist.CheckCalls, Is.EqualTo(0));
        });
    }

    [Test]
    public async Task WishlistCheckJobUsesCacheToAvoidDuplicateEmail()
    {
        var email = new CapturingEmailService();
        using var cache = new MemoryCache(new MemoryCacheOptions());
        cache.Set(
            string.Format(CacheKeys.WishListBackgroundJob, 1),
            new WhishListResponse
            {
                GameName = "Cached Game",
                CurrentPrice = 1,
                IsPriceReached = true
            });
        var wishlist = new FakeWishlistService
        {
            Items =
            [
                new WishListDto
                {
                    Id = 1,
                    Name = "Active Wish",
                    IsActive = true,
                    Price = 5
                }
            ]
        };
        var job = new WishlistCheckJob(
            NullLogger<WishlistCheckJob>.Instance,
            email,
            cache,
            wishlist);
        await job.RunAsync(CancellationToken.None);

        Assert.Multiple(() =>
        {
            Assert.That(email.Messages, Is.Empty);
            Assert.That(wishlist.CheckCalls, Is.EqualTo(0));
        });
    }
}
