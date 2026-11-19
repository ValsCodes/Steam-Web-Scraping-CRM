using Microsoft.AspNetCore.Identity;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Migrations.Operations;
using Microsoft.Extensions.DependencyInjection;
using SteamApp.Application.OperationResults;
using SteamApp.Infrastructure.Context;
using SteamApp.Infrastructure.Identity;
using SteamApp.WebAPI.Migrations;
using SteamApp.WebAPI.Security;
using SteamApp.WebAPI.Services;

namespace SteamApp.IntegrationTests.Security;

[TestFixture]
public sealed class ExternalAccountRegistrationTests
{
    private ServiceProvider? provider;

    [SetUp]
    public async Task CreateDatabaseAsync()
    {
        if (Environment.GetEnvironmentVariable("SteamApp_TestExternalAuthLocalDb") != "1")
        {
            Assert.Ignore("Set SteamApp_TestExternalAuthLocalDb=1 to run against a disposable SQL Server LocalDB catalog.");
        }

        var connection = new SqlConnectionStringBuilder
        {
            DataSource = @"(localdb)\MSSQLLocalDB",
            InitialCatalog = $"SteamAppExternalAuthTest_{Guid.NewGuid():N}",
            IntegratedSecurity = true,
            TrustServerCertificate = true
        };
        var services = new ServiceCollection();
        services.AddLogging();
        services.AddDbContextFactory<ApplicationDbContext>(
            options => options.UseSqlServer(connection.ConnectionString, sql => sql.EnableRetryOnFailure()));
        services.AddIdentityCore<ApplicationUser>(options => options.User.RequireUniqueEmail = true)
            .AddRoles<IdentityRole>()
            .AddEntityFrameworkStores<ApplicationDbContext>();
        services.AddScoped<ExternalAccountRegistration>();
        provider = services.BuildServiceProvider();
        await using var scope = provider.CreateAsyncScope();
        await scope.ServiceProvider.GetRequiredService<ApplicationDbContext>().Database.EnsureCreatedAsync();
        var roles = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();
        Assert.That((await roles.CreateAsync(new IdentityRole(SecurityPolicies.UserRole))).Succeeded, Is.True);
    }

    [TearDown]
    public async Task DropDatabaseAsync()
    {
        if (provider == null)
        {
            return;
        }

        try
        {
            await using var scope = provider.CreateAsyncScope();
            await scope.ServiceProvider.GetRequiredService<ApplicationDbContext>().Database.EnsureDeletedAsync();
        }
        finally
        {
            await provider.DisposeAsync();
            provider = null;
        }
    }

    [TestCase(true)]
    [TestCase(false)]
    public async Task RegisterAsync_ConcurrentDuplicateIdentityOrEmail_CreatesOneUserAndLogin(bool duplicateIdentity)
    {
        await using var scope = provider!.CreateAsyncScope();
        var registration = scope.ServiceProvider.GetRequiredService<ExternalAccountRegistration>();
        var results = await Task.WhenAll(
            registration.RegisterAsync(new("Steam", "76561198000000000", "Steam"), "one@example.test", default),
            registration.RegisterAsync(new("Steam", duplicateIdentity ? "76561198000000000" : "76561198000000001", "Steam"),
                duplicateIdentity ? "two@example.test" : "ONE@example.test", default));

        var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        var users = await db.Users.ToListAsync();
        var logins = await db.UserLogins.ToListAsync();
        var memberships = await db.UserRoles.ToListAsync();
        Assert.Multiple(() =>
        {
            Assert.That(results.Count(result => result.IsSuccess), Is.EqualTo(1));
            Assert.That(results.Single(result => result.IsFailure).Error?.Type, Is.EqualTo(ErrorType.Conflict));
            Assert.That(users, Has.Count.EqualTo(1));
            Assert.That(users.Single().EmailConfirmed, Is.True);
            Assert.That(users.Single().PasswordHash, Is.Null);
            Assert.That(logins.Single().UserId, Is.EqualTo(users.Single().Id));
            Assert.That(memberships.Single().UserId, Is.EqualTo(users.Single().Id));
        });
    }

    [Test]
    public async Task RegisterAsync_WhenLoginInsertFails_RollsBackNewUser()
    {
        await using var scope = provider!.CreateAsyncScope();
        var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        await db.Database.ExecuteSqlRawAsync("""
            CREATE TRIGGER [dbo].[RejectExternalLoginDuringTest] ON [dbo].[AspNetUserLogins]
            INSTEAD OF INSERT AS
                THROW 51020, 'Test login insert failed.', 1;
            """);
        var registration = scope.ServiceProvider.GetRequiredService<ExternalAccountRegistration>();

        Assert.ThrowsAsync<DbUpdateException>(() => registration.RegisterAsync(
            new("Google", "test-subject", "Google"), "new@example.test", default));

        Assert.That(await db.Users.CountAsync(), Is.Zero);
        Assert.That(await db.UserLogins.CountAsync(), Is.Zero);
    }

    [Test]
    public async Task UniqueEmailMigration_WhenReplacingLegacySqlIndex_EnforcesUniqueness()
    {
        await using var scope = provider!.CreateAsyncScope();
        var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        await db.Database.ExecuteSqlRawAsync("""
            DROP INDEX [EmailIndex] ON [dbo].[AspNetUsers];
            CREATE INDEX [EmailIndex] ON [dbo].[AspNetUsers] ([NormalizedEmail]);
            """);
        var migrationSql = new UniqueIdentityEmail().UpOperations.OfType<SqlOperation>().Single().Sql;
        await db.Database.ExecuteSqlRawAsync(migrationSql);
        await db.Database.ExecuteSqlRawAsync(migrationSql);
        db.Users.AddRange(new ApplicationUser { NormalizedEmail = "DUPLICATE@EXAMPLE.TEST" },
            new ApplicationUser { NormalizedEmail = "DUPLICATE@EXAMPLE.TEST" });

        Assert.ThrowsAsync<DbUpdateException>(() => db.SaveChangesAsync());
    }
}
