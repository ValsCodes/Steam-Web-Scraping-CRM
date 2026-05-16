using Microsoft.Extensions.Caching.Memory;
using SteamApp.Application.Caching;
using SteamApp.Domain.Entities;
using SteamApp.Infrastructure.Repositories;
using SteamApp.Tests.TestSupport;

namespace SteamApp.Tests.Repositories;

[TestFixture]
public sealed class SteamRepositoryTests
{
    [Test]
    public async Task GetGameUrl_ReturnsCachedValueWithoutQueryingDatabase()
    {
        using var database = TestDb.CreateDatabase();
        using var cache = TestDb.CreateMemoryCache();
        var cached = new GameUrl { Id = 99, Name = "Cached" };
        cache.Set(string.Format(CacheKeys.GameUrl, cached.Id), cached);

        var repository = new SteamRepository(database.Factory, cache);

        var result = await repository.GetGameUrl(cached.Id);

        Assert.That(result, Is.SameAs(cached));
    }

    [Test]
    public async Task GetGameUrl_LoadsRelatedDataAndCachesResult()
    {
        using var database = TestDb.CreateSeededDatabase();
        using var cache = TestDb.CreateMemoryCache();
        var repository = new SteamRepository(database.Factory, cache);

        var result = await repository.GetGameUrl(2);

        Assert.Multiple(() =>
        {
            Assert.That(result.Game, Is.Not.Null);
            Assert.That(result.GameUrlsPixels, Has.Count.EqualTo(1));
            Assert.That(result.GameUrlsPixels.First().Pixel, Is.Not.Null);
            Assert.That(cache.TryGetValue(string.Format(CacheKeys.GameUrl, 2), out GameUrl? cached), Is.True);
            Assert.That(cached?.Id, Is.EqualTo(2));
        });
    }

    [Test]
    public void GetGameUrl_ThrowsWhenMissing()
    {
        using var database = TestDb.CreateDatabase();
        using var cache = TestDb.CreateMemoryCache();
        var repository = new SteamRepository(database.Factory, cache);

        Assert.That(
            async () => await repository.GetGameUrl(404),
            Throws.Exception.With.Message.Contains("GameUrl with id 404 not found"));
    }

    [Test]
    public async Task GetGame_ReturnsCachedValueWithoutQueryingDatabase()
    {
        using var database = TestDb.CreateDatabase();
        using var cache = TestDb.CreateMemoryCache();
        var cached = new Game { Id = 99, Name = "Cached" };
        cache.Set(string.Format(CacheKeys.Game, cached.Id), cached);

        var repository = new SteamRepository(database.Factory, cache);

        var result = await repository.GetGame(cached.Id);

        Assert.That(result, Is.SameAs(cached));
    }

    [Test]
    public async Task GetGame_LoadsPixelsAndCachesResult()
    {
        using var database = TestDb.CreateSeededDatabase();
        using var cache = TestDb.CreateMemoryCache();
        var repository = new SteamRepository(database.Factory, cache);

        var result = await repository.GetGame(1);

        Assert.Multiple(() =>
        {
            Assert.That(result.Pixels, Has.Count.EqualTo(2));
            Assert.That(cache.TryGetValue(string.Format(CacheKeys.Game, 1), out Game? cached), Is.True);
            Assert.That(cached?.Id, Is.EqualTo(1));
        });
    }

    [Test]
    public void GetGame_ThrowsWhenMissing()
    {
        using var database = TestDb.CreateDatabase();
        using var cache = TestDb.CreateMemoryCache();
        var repository = new SteamRepository(database.Factory, cache);

        Assert.That(
            async () => await repository.GetGame(404),
            Throws.Exception.With.Message.Contains("Game with id 404 not found"));
    }
}
