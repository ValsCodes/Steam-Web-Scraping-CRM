using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using SteamApp.Domain.Entities;
using SteamApp.Infrastructure.Context;
using SteamApp.Infrastructure.Identity;

namespace SteamApp.IntegrationTests.MinimalApis;

[TestFixture]
public sealed class ItemGroupsRelationalIntegrationTests
{
    [Test]
    public async Task ItemGroupConstraints_ExistingGroupSupportsTagsAndPresetsAndRejectsInvalidRows()
    {
        await using var connection = new SqliteConnection("Data Source=:memory:");
        await connection.OpenAsync();
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseSqlite(connection)
            .Options;

        await using (var seedDb = new ApplicationDbContext(options))
        {
            await seedDb.Database.EnsureCreatedAsync();
            seedDb.Users.Add(new ApplicationUser
            {
                Id = "integration-user",
                UserName = "integration-user",
                NormalizedUserName = "INTEGRATION-USER",
            });
            seedDb.Games.Add(new Game
            {
                Id = 1,
                Name = "Game",
                UserId = "integration-user",
            });
            seedDb.ItemGroups.Add(new ItemGroup
            {
                Id = 1,
                GameId = 1,
                Name = "Priority",
                UserId = "integration-user",
            });
            seedDb.Tags.Add(new Tag
            {
                Id = 1,
                GameId = 1,
                ItemGroupId = 1,
                Name = "Grouped tag",
                UserId = "integration-user",
            });
            seedDb.ManualCheckPresets.Add(new ManualCheckPreset
            {
                Id = 1,
                GameId = 1,
                ItemGroupId = 1,
                Name = "Grouped preset",
                CreatedAtUtc = DateTime.UtcNow,
                UpdatedAtUtc = DateTime.UtcNow,
            });
            await seedDb.SaveChangesAsync();
        }

        await using (var duplicateDb = new ApplicationDbContext(options))
        {
            duplicateDb.ItemGroups.Add(new ItemGroup
            {
                GameId = 1,
                Name = "Priority",
                UserId = "integration-user",
            });

            Assert.That(
                async () => await duplicateDb.SaveChangesAsync(),
                Throws.TypeOf<DbUpdateException>());
        }

        await using (var foreignKeyDb = new ApplicationDbContext(options))
        {
            foreignKeyDb.Tags.Add(new Tag
            {
                GameId = 1,
                ItemGroupId = 404,
                Name = "Invalid group",
                UserId = "integration-user",
            });

            Assert.That(
                async () => await foreignKeyDb.SaveChangesAsync(),
                Throws.TypeOf<DbUpdateException>());
        }
    }
}
