using Microsoft.Extensions.Caching.Memory;
using SteamApp.Application.Caching;
using SteamApp.Domain.Entities;
using SteamApp.Infrastructure.Repositories;
using SteamApp.Tests.TestSupport;

namespace SteamApp.Tests.Repositories;

[TestFixture]
public sealed class WishlistRepositoryTests
{
    [Test]
    public async Task GetAsync_ReturnsCachedValueWithoutQueryingDatabase()
    {
        using var database = TestDb.CreateDatabase();
        using var cache = TestDb.CreateMemoryCache();
        var cached = new WishList { Id = 99, Name = "Cached" };
        cache.Set(string.Format(CacheKeys.WishListItem, cached.Id), cached);

        var repository = new WishlistRepository(database.Factory, cache);

        var result = await repository.GetAsync(cached.Id, CancellationToken.None);

        Assert.That(result, Is.SameAs(cached));
    }

    [Test]
    public async Task GetAsync_LoadsGameAndCachesResult()
    {
        using var database = TestDb.CreateSeededDatabase();
        using var cache = TestDb.CreateMemoryCache();
        var repository = new WishlistRepository(database.Factory, cache);

        var result = await repository.GetAsync(1, CancellationToken.None);

        Assert.Multiple(() =>
        {
            Assert.That(result, Is.Not.Null);
            Assert.That(result!.Game, Is.Not.Null);
            Assert.That(result.Game.Name, Is.EqualTo("Alpha Game"));
            Assert.That(cache.TryGetValue(string.Format(CacheKeys.WishListItem, 1), out WishList? cached), Is.True);
            Assert.That(cached?.Id, Is.EqualTo(1));
        });
    }

    [Test]
    public void GetAsync_ThrowsWhenMissing()
    {
        using var database = TestDb.CreateDatabase();
        using var cache = TestDb.CreateMemoryCache();
        var repository = new WishlistRepository(database.Factory, cache);

        Assert.That(
            async () => await repository.GetAsync(404, CancellationToken.None),
            Throws.Exception.With.Message.Contains("WishList with id 404 not found"));
    }

    [Test]
    public async Task GetAllAsync_ReturnsAllWishlistRows()
    {
        using var database = TestDb.CreateSeededDatabase();
        using var cache = TestDb.CreateMemoryCache();
        var repository = new WishlistRepository(database.Factory, cache);

        var result = (await repository.GetAllAsync(CancellationToken.None)).ToArray();

        Assert.That(result.Select(x => x.Id), Is.EquivalentTo(new[] { 1L, 2L }));
    }
}
