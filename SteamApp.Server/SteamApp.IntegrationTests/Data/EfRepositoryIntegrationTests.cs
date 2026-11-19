using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.DependencyInjection;
using SteamApp.Domain.Entities;
using SteamApp.Domain.Enums;
using SteamApp.Infrastructure.Persistence;
using SteamApp.Infrastructure.Persistence.Repositories;
using SteamApp.IntegrationTests.Support;

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
        var migrationsAssembly = typeof(ApplicationDbContext).Assembly;
        var migrations = migrationsAssembly
            .GetTypes()
            .Where(type => type.GetCustomAttributes(typeof(MigrationAttribute), inherit: false).Length > 0)
            .ToArray();
        var hasModelSnapshot = migrationsAssembly
            .GetTypes()
            .Any(type => type.Name == "ApplicationDbContextModelSnapshot");

        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseSqlServer("Server=localhost;Database=OfflineMigrationTest;Integrated Security=true")
            .Options;
        using var db = new ApplicationDbContext(options);

        Assert.Multiple(() =>
        {
            Assert.That(migrations, Is.Not.Empty);
            Assert.That(
                migrations,
                Is.All.Matches<Type>(type =>
                    type.GetCustomAttributes(typeof(DbContextAttribute), inherit: false)
                        .Cast<DbContextAttribute>()
                        .Any(attribute => attribute.ContextType == typeof(ApplicationDbContext))));
            Assert.That(hasModelSnapshot, Is.True);
            Assert.That(db.Database.GetMigrations().Count(), Is.EqualTo(migrations.Length));
            Assert.That(db.Database.HasPendingModelChanges(), Is.False);
        });
    }

    [Test]
    public async Task ScrapeJobClaim_ExpiredClaimCanBeRecoveredAndOnlyItsNewOwnerCanComplete()
    {
        using var factory = new SteamAppFactory();
        await factory.ResetDatabaseAsync();
        using var scope = factory.Services.CreateScope();
        var dbFactory = scope.ServiceProvider.GetRequiredService<IDbContextFactory<ApplicationDbContext>>();
        var history = new ScrapeHistoryDataService(dbFactory);
        var ct = CancellationToken.None;
        var url = await history.GetOwnedGameUrlSnapshotAsync(1, IntegrationSeed.UserId, ct);
        var job = await history.CreateQueuedHistoryAsync(IntegrationSeed.UserId, url!, 2,
            "scrape-page", "Web Scrape", Guid.NewGuid().ToString(), ct);
        var now = DateTime.UtcNow;
        var expiredClaim = Guid.NewGuid();
        var currentClaim = Guid.NewGuid();

        Assert.That(await history.TryClaimJobAsync(job.Id, expiredClaim, now, now.AddMinutes(1), ct), Is.True);
        now = now.AddMinutes(2);
        Assert.That(await history.GetRecoverableJobIdsAsync(now.AddSeconds(-20), now, ct), Does.Contain(job.Id));
        Assert.That(await history.TryClaimJobAsync(job.Id, currentClaim, now, now.AddMinutes(1), ct), Is.True);
        Assert.That(await history.TryClaimJobAsync(job.Id, Guid.NewGuid(), now, now.AddMinutes(1), ct), Is.False);
        Assert.That(await history.CompleteJobAsync(job.Id, expiredClaim, null, "stale failure", ct), Is.False);
        Assert.That(await history.CompleteJobAsync(job.Id, currentClaim, null, null, ct), Is.True);
        Assert.That((await history.GetJobAsync(job.Id, ct))!.Status, Is.EqualTo(ScrapeJobStatusEnum.Succeeded));
    }

    [Test]
    public async Task PresenceLease_ParallelHeartbeatsConvergeOnOneRow()
    {
        using var factory = new SteamAppFactory(sharedDatabase: Guid.NewGuid().ToString("N"));
        await factory.ResetDatabaseAsync();
        using var scope = factory.Services.CreateScope();
        var dbFactory = scope.ServiceProvider.GetRequiredService<IDbContextFactory<ApplicationDbContext>>();
        var service = new AutomationAccessService(dbFactory, TimeProvider.System,
            NullLogger<AutomationAccessService>.Instance);
        var tabId = Guid.NewGuid();

        await Task.WhenAll(Enumerable.Range(0, 8).Select(_ => Task.Run(() =>
            service.UpsertPresenceAsync(IntegrationSeed.UserId, tabId, CancellationToken.None))));

        await using var db = await dbFactory.CreateDbContextAsync();
        var lease = await db.SessionPresenceLeases.SingleAsync(x => x.TabId == tabId);
        Assert.That(lease.UserId, Is.EqualTo(IntegrationSeed.UserId));
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
