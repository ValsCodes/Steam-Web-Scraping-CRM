using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using SteamApp.Domain.Entities;
using SteamApp.Domain.Enums;
using SteamApp.Infrastructure.Identity;
using SteamApp.Tests.TestSupport;
using SteamApp.WebAPI.Exceptions;
using SteamApp.WebAPI.Security;
using SteamApp.WebAPI.Services;

namespace SteamApp.Tests.Services;

[TestFixture]
public sealed class AutomationAccessServiceTests
{
    [Test]
    public async Task UsageCountsOnlyTimeAfterRollingAndResetCutoffs()
    {
        using var database = TestDb.CreateDatabase();
        var now = new DateTimeOffset(2026, 10, 7, 8, 0, 0, TimeSpan.Zero);
        var service = CreateService(database, now);
        var run = AddRun(database, "user-1");
        var policy = database.Context.AutomationPolicies.Single();
        policy.NonAdminLimitSeconds = 900;
        policy.UsageResetAtUtc = now.UtcDateTime.AddMinutes(-10);
        database.Context.AutomationUsageIntervals.AddRange(
            new AutomationUsageInterval
            {
                UserId = "user-1",
                ManualCheckRunId = run.Id,
                StartedAtUtc = now.UtcDateTime.AddHours(-25),
                EndedAtUtc = now.UtcDateTime.AddHours(-24).AddMinutes(5)
            },
            new AutomationUsageInterval
            {
                UserId = "user-1",
                ManualCheckRunId = run.Id,
                StartedAtUtc = now.UtcDateTime.AddMinutes(-20),
                EndedAtUtc = now.UtcDateTime.AddMinutes(-5)
            });
        database.Context.SaveChanges();

        var usage = await service.GetUsageAsync("user-1", CancellationToken.None);

        Assert.Multiple(() =>
        {
            Assert.That(usage.UsedSeconds, Is.EqualTo(300));
            Assert.That(usage.RemainingSeconds, Is.EqualTo(600));
            Assert.That(usage.UsageResetAtUtc, Is.EqualTo(policy.UsageResetAtUtc));
        });
    }

    [Test]
    public async Task PresenceRemainsActiveUntilTheLastTabLeaseIsReleased()
    {
        using var database = TestDb.CreateDatabase();
        var service = CreateService(database, new DateTimeOffset(2026, 10, 7, 8, 0, 0, TimeSpan.Zero));
        var first = Guid.NewGuid();
        var second = Guid.NewGuid();

        await service.UpsertPresenceAsync("user-1", first, CancellationToken.None);
        await service.UpsertPresenceAsync("user-1", second, CancellationToken.None);
        await service.ReleasePresenceAsync("user-1", first, CancellationToken.None);
        var oneTabLeft = await service.GetUsageAsync("user-1", CancellationToken.None);
        await service.ReleasePresenceAsync("user-1", second, CancellationToken.None);
        var noTabsLeft = await service.GetUsageAsync("user-1", CancellationToken.None);

        Assert.Multiple(() =>
        {
            Assert.That(oneTabLeft.PresenceActive, Is.True);
            Assert.That(noTabsLeft.PresenceActive, Is.False);
        });
    }

    [Test]
    public async Task ExpiredPresenceLeaseDoesNotAuthorizeExecution()
    {
        using var database = TestDb.CreateDatabase();
        var now = new DateTimeOffset(2026, 10, 7, 8, 0, 0, TimeSpan.Zero);
        database.Context.SessionPresenceLeases.Add(new SessionPresenceLease
        {
            UserId = "user-1",
            TabId = Guid.NewGuid(),
            UpdatedAtUtc = now.UtcDateTime.AddMinutes(-1),
            ExpiresAtUtc = now.UtcDateTime.AddSeconds(-1)
        });
        database.Context.SaveChanges();
        var service = CreateService(database, now);

        var usage = await service.GetUsageAsync("user-1", CancellationToken.None);
        var exception = Assert.ThrowsAsync<AutomationAccessException>(() =>
            service.EnsureCanExecuteAsync("user-1", CancellationToken.None));

        Assert.Multiple(() =>
        {
            Assert.That(usage.PresenceActive, Is.False);
            Assert.That(exception!.StatusCode, Is.EqualTo(409));
        });
    }

    [Test]
    public async Task UserCannotReleaseAnotherUsersTabLease()
    {
        using var database = TestDb.CreateDatabase();
        var service = CreateService(database, new DateTimeOffset(2026, 10, 7, 8, 0, 0, TimeSpan.Zero));
        var tabId = Guid.NewGuid();
        await service.UpsertPresenceAsync("user-1", tabId, CancellationToken.None);

        await service.ReleasePresenceAsync("user-2", tabId, CancellationToken.None);

        var usage = await service.GetUsageAsync("user-1", CancellationToken.None);
        Assert.That(usage.PresenceActive, Is.True);
    }

    [Test]
    public async Task UserCannotHeartbeatAnotherUsersTabIdentifier()
    {
        using var database = TestDb.CreateDatabase();
        var service = CreateService(database, new DateTimeOffset(2026, 10, 7, 8, 0, 0, TimeSpan.Zero));
        var tabId = Guid.NewGuid();
        await service.UpsertPresenceAsync("user-1", tabId, CancellationToken.None);

        var exception = Assert.ThrowsAsync<AutomationAccessException>(() =>
            service.UpsertPresenceAsync("user-2", tabId, CancellationToken.None));

        Assert.That(exception!.StatusCode, Is.EqualTo(409));
    }

    [Test]
    public async Task ExecutionRequiresPresenceAndThenEnforcesQuota()
    {
        using var database = TestDb.CreateDatabase();
        var now = new DateTimeOffset(2026, 10, 7, 8, 0, 0, TimeSpan.Zero);
        var service = CreateService(database, now);
        var noPresence = Assert.ThrowsAsync<AutomationAccessException>(() =>
            service.EnsureCanExecuteAsync("user-1", CancellationToken.None));
        await service.UpsertPresenceAsync("user-1", Guid.NewGuid(), CancellationToken.None);
        var run = AddRun(database, "user-1");
        var policy = database.Context.AutomationPolicies.Single();
        policy.NonAdminLimitSeconds = 60;
        database.Context.AutomationUsageIntervals.Add(new AutomationUsageInterval
        {
            UserId = "user-1",
            ManualCheckRunId = run.Id,
            StartedAtUtc = now.UtcDateTime.AddMinutes(-2),
            EndedAtUtc = now.UtcDateTime
        });
        database.Context.SaveChanges();

        var exhausted = Assert.ThrowsAsync<AutomationAccessException>(() =>
            service.EnsureCanExecuteAsync("user-1", CancellationToken.None));

        Assert.Multiple(() =>
        {
            Assert.That(noPresence!.StatusCode, Is.EqualTo(409));
            Assert.That(exhausted!.StatusCode, Is.EqualTo(429));
            Assert.That(exhausted.RetryAtUtc, Is.Not.Null);
        });
    }

    [Test]
    public async Task ZeroLimitDeniesExecutionEvenWithActivePresence()
    {
        using var database = TestDb.CreateDatabase();
        var service = CreateService(database, new DateTimeOffset(2026, 10, 7, 8, 0, 0, TimeSpan.Zero));
        database.Context.AutomationPolicies.Single().NonAdminLimitSeconds = 0;
        database.Context.SaveChanges();
        await service.UpsertPresenceAsync("user-1", Guid.NewGuid(), CancellationToken.None);

        var usage = await service.GetUsageAsync("user-1", CancellationToken.None);
        var exception = Assert.ThrowsAsync<AutomationAccessException>(() =>
            service.EnsureCanExecuteAsync("user-1", CancellationToken.None));

        Assert.Multiple(() =>
        {
            Assert.That(usage.RemainingSeconds, Is.Zero);
            Assert.That(exception!.StatusCode, Is.EqualTo(429));
            Assert.That(exception.RetryAtUtc, Is.Null);
        });
    }

    [Test]
    public async Task AdministratorsAreUnlimitedAndDoNotRequirePresence()
    {
        using var database = TestDb.CreateDatabase();
        database.Context.Users.Add(new ApplicationUser { Id = "admin-1", UserName = "admin@example.test" });
        database.Context.Roles.Add(new IdentityRole { Id = "role-admin", Name = SecurityPolicies.AdminRole, NormalizedName = "ADMIN" });
        database.Context.UserRoles.Add(new IdentityUserRole<string> { UserId = "admin-1", RoleId = "role-admin" });
        database.Context.SaveChanges();
        var service = CreateService(database, new DateTimeOffset(2026, 10, 7, 8, 0, 0, TimeSpan.Zero));

        var usage = await service.GetUsageAsync("admin-1", CancellationToken.None);
        Assert.DoesNotThrowAsync(() => service.EnsureCanExecuteAsync("admin-1", CancellationToken.None));

        Assert.Multiple(() =>
        {
            Assert.That(usage.Unlimited, Is.True);
            Assert.That(usage.PresenceRequired, Is.False);
            Assert.That(usage.RemainingSeconds, Is.Null);
        });
    }

    private static AutomationAccessService CreateService(TestDatabase database, DateTimeOffset now)
    {
        var clock = new Mock<TimeProvider>();
        clock.Setup(x => x.GetUtcNow()).Returns(now);
        return new AutomationAccessService(
            database.Factory,
            clock.Object,
            NullLogger<AutomationAccessService>.Instance);
    }

    private static ManualCheckRun AddRun(TestDatabase database, string userId)
    {
        var run = new ManualCheckRun
        {
            UserId = userId,
            GameId = 1,
            GameUrlId = 1,
            PresetName = "Usage test",
            SetupJson = "{}",
            Status = ManualCheckRunStatusEnum.Succeeded,
            Date = DateTime.UtcNow,
            CorrelationId = Guid.NewGuid().ToString("N")
        };
        database.Context.ManualCheckRuns.Add(run);
        database.Context.SaveChanges();
        return run;
    }
}
