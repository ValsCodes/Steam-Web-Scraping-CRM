using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using SteamApp.Application.DTOs.ManualCheck;
using SteamApp.Domain.Entities;
using SteamApp.Domain.Enums;
using SteamApp.Infrastructure.Context;
using SteamApp.Infrastructure.Identity;
using SteamApp.WebAPI.Services;

namespace SteamApp.IntegrationTests.Data;

[TestFixture]
public sealed class ManualCheckPresetRelationsIntegrationTests
{
    [Test]
    public async Task PresetAssignments_ReconcileOnlyChangedRowsAndRollBackAfterInsertFailure()
    {
        await using var connection = new SqliteConnection("Data Source=:memory:");
        await connection.OpenAsync();
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseSqlite(connection)
            .Options;

        await SeedAsync(options);
        var service = new ManualCheckDataService(new PooledDbContextFactory<ApplicationDbContext>(options));
        var created = await service.CreatePresetAsync(Input("Initial", [1, 2]), CancellationToken.None);

        await using (var triggerDb = new ApplicationDbContext(options))
        {
            await triggerDb.Database.ExecuteSqlRawAsync(
                """
                CREATE TABLE relation_audit (operation TEXT NOT NULL, game_url_id INTEGER NOT NULL);
                CREATE TRIGGER audit_relation_insert AFTER INSERT ON manual_check_preset_game_url
                BEGIN
                    INSERT INTO relation_audit (operation, game_url_id) VALUES ('insert', NEW.game_url_id);
                END;
                CREATE TRIGGER audit_relation_delete AFTER DELETE ON manual_check_preset_game_url
                BEGIN
                    INSERT INTO relation_audit (operation, game_url_id) VALUES ('delete', OLD.game_url_id);
                END;
                """);
        }

        var updated = await service.UpdatePresetAsync(
            created.Id,
            Input("Updated", [2, 3]),
            CancellationToken.None);

        await using (var verifyDeltaDb = new ApplicationDbContext(options))
        {
            var assignments = await verifyDeltaDb.ManualCheckPresetGameUrls
                .Where(x => x.ManualCheckPresetId == created.Id)
                .OrderBy(x => x.GameUrlId)
                .Select(x => x.GameUrlId)
                .ToListAsync();
            var audit = await ReadAuditAsync(verifyDeltaDb);
            Assert.Multiple(() =>
            {
                Assert.That(updated.GameUrlIds, Is.EqualTo(new long[] { 2, 3 }));
                Assert.That(assignments, Is.EqualTo(new long[] { 2, 3 }));
                Assert.That(audit, Is.EqualTo(new[] { "delete:1", "insert:3" }));
            });

            await verifyDeltaDb.Database.ExecuteSqlRawAsync(
                """
                CREATE TRIGGER reject_relation_insert BEFORE INSERT ON manual_check_preset_game_url
                WHEN NEW.game_url_id = 1
                BEGIN
                    SELECT RAISE(ABORT, 'forced relation failure');
                END;
                """);
        }

        Assert.That(
            async () => await service.UpdatePresetAsync(
                created.Id,
                Input("Must roll back", [1, 2]),
                CancellationToken.None),
            Throws.Exception);

        await using var verifyRollbackDb = new ApplicationDbContext(options);
        var storedPreset = await verifyRollbackDb.ManualCheckPresets.SingleAsync(x => x.Id == created.Id);
        var storedAssignments = await verifyRollbackDb.ManualCheckPresetGameUrls
            .Where(x => x.ManualCheckPresetId == created.Id)
            .OrderBy(x => x.GameUrlId)
            .Select(x => x.GameUrlId)
            .ToListAsync();
        Assert.Multiple(() =>
        {
            Assert.That(storedPreset.Name, Is.EqualTo("Updated"));
            Assert.That(storedAssignments, Is.EqualTo(new long[] { 2, 3 }));
        });
    }

    [Test]
    public async Task PresetAssignments_RejectEmptyDuplicateAndIncompatibleUrlsWithoutMutation()
    {
        await using var connection = new SqliteConnection("Data Source=:memory:");
        await connection.OpenAsync();
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseSqlite(connection)
            .Options;

        await SeedAsync(options);
        var service = new ManualCheckDataService(new PooledDbContextFactory<ApplicationDbContext>(options));
        var created = await service.CreatePresetAsync(Input("Initial", [1]), CancellationToken.None);

        Assert.Multiple(() =>
        {
            Assert.That(
                async () => await service.UpdatePresetAsync(created.Id, Input("Empty", []), CancellationToken.None),
                Throws.Exception);
            Assert.That(
                async () => await service.UpdatePresetAsync(created.Id, Input("Duplicate", [1, 1]), CancellationToken.None),
                Throws.Exception);
            Assert.That(
                async () => await service.UpdatePresetAsync(created.Id, Input("Wrong mode", [4]), CancellationToken.None),
                Throws.Exception);
            Assert.That(
                async () => await service.UpdatePresetAsync(created.Id, Input("Wrong game", [5]), CancellationToken.None),
                Throws.Exception);
            Assert.That(
                async () => await service.UpdatePresetAsync(created.Id, Input("Foreign owner", [6]), CancellationToken.None),
                Throws.Exception);
        });

        await using var verifyDb = new ApplicationDbContext(options);
        var storedPreset = await verifyDb.ManualCheckPresets.SingleAsync(x => x.Id == created.Id);
        var storedAssignments = await verifyDb.ManualCheckPresetGameUrls
            .Where(x => x.ManualCheckPresetId == created.Id)
            .Select(x => x.GameUrlId)
            .ToListAsync();
        Assert.Multiple(() =>
        {
            Assert.That(storedPreset.Name, Is.EqualTo("Initial"));
            Assert.That(storedAssignments, Is.EqualTo(new long[] { 1 }));
        });
    }

    private static ManualCheckPresetWriteDto Input(string name, IReadOnlyList<long> gameUrlIds)
    {
        return new ManualCheckPresetWriteDto
        {
            GameId = 1,
            Name = name,
            ListingLimit = 10,
            GameUrlIds = gameUrlIds.ToList(),
            Criteria = [new ManualCheckCriterionDto { NameContains = "Exterior" }]
        };
    }

    private static async Task SeedAsync(DbContextOptions<ApplicationDbContext> options)
    {
        await using var db = new ApplicationDbContext(options);
        await db.Database.EnsureCreatedAsync();
        db.Users.AddRange(
            User("owner"),
            User("other-owner"));
        db.Games.AddRange(
            new Game { Id = 1, Name = "Owned game", UserId = "owner" },
            new Game { Id = 2, Name = "Other owned game", UserId = "owner" },
            new Game { Id = 3, Name = "Foreign game", UserId = "other-owner" });
        db.GameUrls.AddRange(
            Url(1, 1, "owner", ScrapingModeEnum.ManualBatch, isActive: true),
            Url(2, 1, "owner", ScrapingModeEnum.ManualBatch, isActive: false),
            Url(3, 1, "owner", ScrapingModeEnum.ManualBatch, isActive: true),
            Url(4, 1, "owner", ScrapingModeEnum.Batch, isActive: true),
            Url(5, 2, "owner", ScrapingModeEnum.ManualBatch, isActive: true),
            Url(6, 3, "other-owner", ScrapingModeEnum.ManualBatch, isActive: true));
        await db.SaveChangesAsync();
    }

    private static ApplicationUser User(string id) => new()
    {
        Id = id,
        UserName = id,
        NormalizedUserName = id.ToUpperInvariant()
    };

    private static GameUrl Url(
        long id,
        long gameId,
        string userId,
        ScrapingModeEnum mode,
        bool isActive) => new()
        {
            Id = id,
            GameId = gameId,
            UserId = userId,
            Name = $"URL {id}",
            ScrapingModeId = (long)mode,
            IsActive = isActive
        };

    private static async Task<string[]> ReadAuditAsync(ApplicationDbContext db)
    {
        await using var command = db.Database.GetDbConnection().CreateCommand();
        command.CommandText = "SELECT operation, game_url_id FROM relation_audit ORDER BY rowid";
        await using var reader = await command.ExecuteReaderAsync();
        var rows = new List<string>();
        while (await reader.ReadAsync())
        {
            rows.Add($"{reader.GetString(0)}:{reader.GetInt64(1)}");
        }
        return rows.ToArray();
    }
}
