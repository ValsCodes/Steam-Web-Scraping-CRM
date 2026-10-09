using System.Data.Common;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Storage;
using SteamApp.Application.DTOs.ManualCheck;
using SteamApp.Domain.Entities;
using SteamApp.Domain.Enums;
using SteamApp.Infrastructure.Context;
using SteamApp.Infrastructure.Identity;
using SteamApp.WebAPI.Services;

namespace SteamApp.IntegrationTests.Data;

[TestFixture]
public sealed class ManualCheckRunTransactionsIntegrationTests
{
    [Test]
    public async Task RunStateTransactions_ExecuteWithinConfiguredStrategyAndPreserveProductTrace()
    {
        await using var connection = new SqliteConnection("Data Source=:memory:");
        await connection.OpenAsync();
        var interceptor = new ExecutionStrategyTransactionInterceptor();
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseSqlite(connection, sqlite => sqlite.ExecutionStrategy(
                dependencies => new TestRetryingExecutionStrategy(dependencies)))
            .AddInterceptors(interceptor)
            .Options;

        await SeedAsync(options);
        interceptor.IsEnabled = true;
        var service = new ManualCheckDataService(new PooledDbContextFactory<ApplicationDbContext>(options));
        var run = await service.CreateRunFromSetupAsync(Setup(), CancellationToken.None);

        await service.MarkRunningAndGetRunAsync(run.Id, CancellationToken.None);
        await service.PauseAsync(run.Id, CancellationToken.None);
        await service.MarkPausedAsync(run.Id, CancellationToken.None);
        await service.ContinueAsync(run.Id, CancellationToken.None);
        await service.MarkRunningAndGetRunAsync(run.Id, CancellationToken.None);

        var results = new ManualCheckRunResultsDto
        {
            ProductTraces =
            [
                new ManualCheckProductTraceDto
                {
                    ProductId = 1,
                    ProductName = "Rocket Launcher",
                    FullUrl = "https://steamcommunity.com/market/listings/440/Rocket%20Launcher",
                    MatchEvaluated = true,
                    SteamApiResultJson = "{\"success\":true}"
                }
            ]
        };
        await service.UpdateProgressAsync(run.Id, 1, 0, 0, results, CancellationToken.None);
        await service.CompleteAsync(
            run.Id,
            ManualCheckRunStatusEnum.Succeeded,
            results,
            CancellationToken.None);

        var completed = await service.GetRunAsync(run.Id, CancellationToken.None);
        await using var verifyDb = new ApplicationDbContext(options);
        var usageIntervals = await verifyDb.AutomationUsageIntervals
            .Where(x => x.ManualCheckRunId == run.Id)
            .ToListAsync();

        Assert.Multiple(() =>
        {
            Assert.That(interceptor.StartedTransactions, Is.EqualTo(5));
            Assert.That(completed!.Status, Is.EqualTo(ManualCheckRunStatusEnum.Succeeded));
            Assert.That(completed.Results.ProductTraces, Has.Count.EqualTo(1));
            Assert.That(completed.Results.ProductTraces[0].SteamApiResultJson, Does.Contain("success"));
            Assert.That(usageIntervals, Has.Count.EqualTo(2));
            Assert.That(usageIntervals, Has.All.Matches<AutomationUsageInterval>(x => x.EndedAtUtc.HasValue));
        });
    }

    private static async Task SeedAsync(DbContextOptions<ApplicationDbContext> options)
    {
        await using var db = new ApplicationDbContext(options);
        await db.Database.EnsureCreatedAsync();
        db.Users.Add(new ApplicationUser
        {
            Id = "owner",
            UserName = "owner",
            NormalizedUserName = "OWNER"
        });
        db.Games.Add(new Game { Id = 1, Name = "Team Fortress 2", UserId = "owner" });
        db.GameUrls.Add(new GameUrl
        {
            Id = 1,
            GameId = 1,
            UserId = "owner",
            Name = "Steam Market",
            ScrapingModeId = (long)ScrapingModeEnum.ManualBatch,
            IsActive = true
        });
        await db.SaveChangesAsync();
    }

    private static ManualCheckSetupDto Setup() => new()
    {
        UserId = "owner",
        PresetName = "Queue child",
        GameId = 1,
        GameUrlId = 1,
        ListingLimit = 10,
        Products =
        [
            new ManualCheckProductInputDto
            {
                ProductId = 1,
                ProductName = "Rocket Launcher",
                GameUrlId = 1,
                GameUrlName = "Steam Market",
                FullUrl = "https://steamcommunity.com/market/listings/440/Rocket%20Launcher"
            }
        ]
    };

    private sealed class ExecutionStrategyTransactionInterceptor : DbTransactionInterceptor
    {
        public bool IsEnabled { get; set; }
        public int StartedTransactions { get; private set; }

        public override ValueTask<InterceptionResult<DbTransaction>> TransactionStartingAsync(
            DbConnection connection,
            TransactionStartingEventData eventData,
            InterceptionResult<DbTransaction> result,
            CancellationToken cancellationToken = default)
        {
            if (IsEnabled && ExecutionStrategy.Current is null)
            {
                throw new InvalidOperationException("The transaction was started outside the configured execution strategy.");
            }

            if (IsEnabled)
            {
                StartedTransactions++;
            }
            return ValueTask.FromResult(result);
        }
    }

    private sealed class TestRetryingExecutionStrategy(ExecutionStrategyDependencies dependencies)
        : ExecutionStrategy(dependencies, 1, TimeSpan.Zero)
    {
        protected override bool ShouldRetryOn(Exception exception) => false;
    }
}
