using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.DependencyInjection;
using SteamApp.Domain.Entities;
using SteamApp.Domain.Enums;
using SteamApp.Infrastructure.Context;
using SteamApp.Infrastructure.Repositories;
using SteamApp.IntegrationTests.Support;
using SteamApp.WebAPI;

namespace SteamApp.IntegrationTests.Data;

[TestFixture]
public sealed class EfRepositoryIntegrationTests
{
    [Test]
    public async Task EfModelCreatesSchemaAndSeedsScrapingModes()
    {
        using var factory = new SteamAppFactory();
        using var client = factory.CreateAuthenticatedClient();
        await factory.ResetDatabaseAsync();

        using var scope = factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

        Assert.Multiple(() =>
        {
            Assert.That(db.Database.ProviderName, Does.Contain("Sqlite"));
            Assert.That(db.ScrapingModes.Count(), Is.EqualTo(4));
            Assert.That(db.ScrapingModes.Any(x => x.Id == (long)ScrapingModeEnum.PublicApi), Is.True);
        });
    }

    [Test]
    public void MigrationMetadataIsAvailableForProductionDatabaseUpgrades()
    {
        var webApiAssembly = typeof(Program).Assembly;
        var migrations = webApiAssembly
            .GetTypes()
            .Where(type => type.GetCustomAttributes(typeof(MigrationAttribute), inherit: false).Length > 0)
            .ToArray();
        var hasModelSnapshot = webApiAssembly
            .GetTypes()
            .Any(type => type.Name == "ApplicationDbContextModelSnapshot");

        Assert.Multiple(() =>
        {
            Assert.That(migrations, Is.Not.Empty);
            Assert.That(hasModelSnapshot, Is.True);
        });
    }

    [Test]
    public async Task RepositoriesLoadRequiredNavigationProperties()
    {
        using var factory = new SteamAppFactory();
        using var client = factory.CreateAuthenticatedClient();
        await factory.ResetDatabaseAsync();

        using var scope = factory.Services.CreateScope();
        var dbFactory = scope.ServiceProvider.GetRequiredService<IDbContextFactory<ApplicationDbContext>>();
        using var cache = new MemoryCache(new MemoryCacheOptions());
        var steamRepository = new SteamRepository(dbFactory, cache);
        var wishlistRepository = new WishlistRepository(dbFactory, cache);

        var gameUrl = await steamRepository.GetGameUrl(2);
        var game = await steamRepository.GetGame(1);
        var wish = await wishlistRepository.GetAsync(1, CancellationToken.None);

        Assert.Multiple(() =>
        {
            Assert.That(gameUrl.Game, Is.Not.Null);
            Assert.That(gameUrl.GameUrlsPixels.First().Pixel.Name, Is.EqualTo("Team Spirit"));
            Assert.That(game.Pixels, Has.Count.EqualTo(2));
            Assert.That(wish?.Game.Name, Is.EqualTo("Alpha Game"));
        });
    }

    [Test]
    public async Task CompositeKeysPreventDuplicateJoinRows()
    {
        using var factory = new SteamAppFactory();
        using var client = factory.CreateAuthenticatedClient();
        await factory.ResetDatabaseAsync();

        using var scope = factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        db.ProductTags.Add(new ProductTags { ProductId = 1, TagId = 1 });

        Assert.That(
            async () => await db.SaveChangesAsync(),
            Throws.InstanceOf<DbUpdateException>());
    }

    [Test]
    public async Task DeleteRestrictionsPreventRemovingReferencedGame()
    {
        using var factory = new SteamAppFactory();
        using var client = factory.CreateAuthenticatedClient();
        await factory.ResetDatabaseAsync();

        using var scope = factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        db.Games.Remove(new Game { Id = 1 });

        Assert.That(
            async () => await db.SaveChangesAsync(),
            Throws.Exception);
    }

    [Test]
    public async Task ReadQueriesDoNotTrackProjectedEndpointEntities()
    {
        using var factory = new SteamAppFactory();
        using var client = factory.CreateAuthenticatedClient();
        await factory.ResetDatabaseAsync();

        var response = await client.GetAsync("/api/game-urls/");

        using var scope = factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

        Assert.Multiple(() =>
        {
            Assert.That(response.StatusCode, Is.EqualTo(System.Net.HttpStatusCode.OK));
            Assert.That(db.ChangeTracker.Entries<GameUrl>(), Is.Empty);
        });
    }
}
