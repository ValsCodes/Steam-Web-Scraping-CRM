using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using SteamApp.Domain.Entities;
using SteamApp.Domain.Enums;
using SteamApp.Infrastructure.Context;

namespace SteamApp.IntegrationTests.Data;

[TestFixture]
public sealed class AutomaticQueueRelationalIntegrationTests
{
    [Test]
    public async Task QueueConstraints_EnforceOneActiveRunAndPreserveHistoryWhenDefinitionIsDeleted()
    {
        await using var connection = new SqliteConnection("Data Source=:memory:");
        await connection.OpenAsync();
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseSqlite(connection)
            .Options;

        long definitionId;
        long firstRunId;
        await using (var seedDb = new ApplicationDbContext(options))
        {
            await seedDb.Database.EnsureCreatedAsync();
            var definition = new AutomaticQueueDefinition
            {
                UserId = "owner",
                Name = "Queue",
                CreatedAtUtc = DateTime.UtcNow,
                UpdatedAtUtc = DateTime.UtcNow,
                Blocks =
                [
                    new AutomaticQueueBlock
                    {
                        BlockKey = Guid.NewGuid(),
                        SortOrder = 0,
                        BlockType = AutomaticQueueBlockTypeEnum.Delay,
                        ConfigurationJson = "{}"
                    }
                ]
            };
            seedDb.AutomaticQueueDefinitions.Add(definition);
            await seedDb.SaveChangesAsync();
            definitionId = definition.Id;

            var run = Run(definitionId, AutomaticQueueRunStatusEnum.Running);
            seedDb.AutomaticQueueRuns.Add(run);
            await seedDb.SaveChangesAsync();
            firstRunId = run.Id;
        }

        await using (var duplicateDb = new ApplicationDbContext(options))
        {
            duplicateDb.AutomaticQueueRuns.Add(Run(definitionId, AutomaticQueueRunStatusEnum.Paused));

            Assert.That(
                async () => await duplicateDb.SaveChangesAsync(),
                Throws.TypeOf<DbUpdateException>());
        }

        await using (var replaceDb = new ApplicationDbContext(options))
        {
            var first = await replaceDb.AutomaticQueueRuns.SingleAsync(x => x.Id == firstRunId);
            first.Status = AutomaticQueueRunStatusEnum.Succeeded;
            first.CompletedAtUtc = DateTime.UtcNow;
            replaceDb.AutomaticQueueRuns.Add(Run(definitionId, AutomaticQueueRunStatusEnum.Queued));
            await replaceDb.SaveChangesAsync();
        }

        await using (var deleteDb = new ApplicationDbContext(options))
        {
            var definition = await deleteDb.AutomaticQueueDefinitions.SingleAsync(x => x.Id == definitionId);
            deleteDb.AutomaticQueueDefinitions.Remove(definition);
            await deleteDb.SaveChangesAsync();
        }

        await using (var verifyDb = new ApplicationDbContext(options))
        {
            var runs = await verifyDb.AutomaticQueueRuns.OrderBy(x => x.Id).ToListAsync();
            var definitionBlockCount = await verifyDb.AutomaticQueueBlocks.CountAsync();
            var runBlockCount = await verifyDb.AutomaticQueueRunBlocks.CountAsync();
            Assert.Multiple(() =>
            {
                Assert.That(runs, Has.Count.EqualTo(2));
                Assert.That(runs.All(x => x.AutomaticQueueDefinitionId is null), Is.True);
                Assert.That(runs.All(x => x.QueueName == "Queue"), Is.True);
                Assert.That(definitionBlockCount, Is.Zero);
                Assert.That(runBlockCount, Is.EqualTo(2));
            });
        }
    }

    private static AutomaticQueueRun Run(long definitionId, AutomaticQueueRunStatusEnum status)
    {
        return new AutomaticQueueRun
        {
            AutomaticQueueDefinitionId = definitionId,
            UserId = "owner",
            QueueName = "Queue",
            Status = status,
            Date = DateTime.UtcNow,
            CorrelationId = Guid.NewGuid().ToString("N"),
            Blocks =
            [
                new AutomaticQueueRunBlock
                {
                    BlockKey = Guid.NewGuid(),
                    SortOrder = 0,
                    BlockType = AutomaticQueueBlockTypeEnum.Delay,
                    Status = AutomaticQueueBlockRunStatusEnum.Pending,
                    SetupJson = "{}"
                }
            ]
        };
    }
}
