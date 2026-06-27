using System.Net.Http.Headers;
using System.Security.Cryptography;
using System.Text;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using SteamApp.Application.Services;
using SteamApp.Infrastructure.Context;
using SteamApp.Interfaces.Services;
using SteamApp.WebAPI;
using SteamApp.WebAPI.Security;

namespace SteamApp.IntegrationTests.Support;

public sealed class SteamAppFactory : WebApplicationFactory<Program>
{
    private readonly Dictionary<string, string?> originalEnvironment = new(StringComparer.Ordinal);
    private readonly Dictionary<string, string?> environmentValues;
    private readonly SqliteConnection connection;

    public SteamAppFactory(
        string environmentName = "IntegrationTesting",
        IReadOnlyDictionary<string, string?>? overrides = null,
        bool applyDefaultConfiguration = true)
    {
        EnvironmentName = environmentName;
        SteamService = new FakeSteamService();
        WishlistService = new FakeWishlistService();
        EmailService = new CapturingEmailService();
        connection = new SqliteConnection("Data Source=:memory:");
        connection.Open();

        environmentValues = applyDefaultConfiguration
            ? CreateDefaultEnvironment()
            : new Dictionary<string, string?>(StringComparer.Ordinal);

        if (overrides != null)
        {
            foreach (var (key, value) in overrides)
            {
                environmentValues[key] = value;
            }
        }

        ApplyEnvironment();
    }

    public string EnvironmentName { get; }
    public FakeSteamService SteamService { get; }
    public FakeWishlistService WishlistService { get; }
    public CapturingEmailService EmailService { get; }

    public HttpClient CreateAnonymousClient()
    {
        return CreateClient(new WebApplicationFactoryClientOptions
        {
            AllowAutoRedirect = false,
            BaseAddress = new Uri("https://localhost")
        });
    }

    public HttpClient CreateAuthenticatedClient(
        string scope = SecurityPolicies.UserScope,
        DateTime? expiresUtc = null)
    {
        var client = CreateAnonymousClient();
        client.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue(
                "Bearer",
                IntegrationJwt.CreateToken(scope, expiresUtc));

        return client;
    }

    public Task ResetDatabaseAsync()
    {
        SteamService.Reset();
        WishlistService.Reset();
        EmailService.Clear();
        return IntegrationSeed.SeedAsync(Services);
    }

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment(EnvironmentName);
        builder.ConfigureLogging(logging => logging.ClearProviders());
        builder.ConfigureServices(services =>
        {
            services.RemoveAll<IDatabaseProvider>();
            services.RemoveAll<IDbContextOptionsConfiguration<ApplicationDbContext>>();
            services.RemoveAll<DbContextOptions>();
            services.RemoveAll<DbContextOptions<ApplicationDbContext>>();
            services.RemoveAll<IDbContextFactory<ApplicationDbContext>>();
            services.RemoveAll<ApplicationDbContext>();
            services.AddDbContextFactory<ApplicationDbContext>(opts =>
                opts.UseSqlite(connection));
            services.Configure<HttpsRedirectionOptions>(opts =>
                opts.HttpsPort = 443);

            services.RemoveAll<ISteamService>();
            services.AddSingleton<ISteamService>(SteamService);

            services.RemoveAll<IWishlistService>();
            services.AddSingleton<IWishlistService>(WishlistService);

            services.RemoveAll<IEmailService>();
            services.AddSingleton<IEmailService>(EmailService);
        });
    }

    protected override void Dispose(bool disposing)
    {
        base.Dispose(disposing);

        if (disposing)
        {
            connection.Dispose();
            RestoreEnvironment();
        }
    }

    private static Dictionary<string, string?> CreateDefaultEnvironment()
    {
        return new Dictionary<string, string?>(StringComparer.Ordinal)
        {
            ["ConnectionStrings__DefaultConnection"] =
                "Server=(localdb)\\MSSQLLocalDB;Database=SteamAppIntegrationTests;Trusted_Connection=True;TrustServerCertificate=True;",
            ["JwtSettings__Key"] = IntegrationJwt.Key,
            ["JwtSettings__Issuer"] = IntegrationJwt.Issuer,
            ["JwtSettings__Audience"] = IntegrationJwt.Audience,
            ["JwtSettings__DurationMinutes"] = "60",
            ["AllowedHosts"] = "localhost;127.0.0.1;app.example.test",
            ["Cors__AllowedOrigins__0"] = "https://spa.example.test",
            ["Authentication__AllowRegistration"] = "true",
            ["Database__EnsureIdentitySchemaOnStartup"] = "false",
            ["Workers__WishlistCheck__Enabled"] = "false",
            ["Clients__0__ClientId"] = "integration-client",
            ["Clients__0__ClientSecretHash"] = Sha256Hex("integration-secret"),
            ["Clients__0__AllowedScope"] = SecurityPolicies.UserScope,
            ["Clients__1__ClientId"] = "internal-client",
            ["Clients__1__ClientSecretHash"] = Sha256Hex("internal-secret"),
            ["Clients__1__AllowedScope"] = SecurityPolicies.InternalScope
        };
    }

    private void ApplyEnvironment()
    {
        foreach (var (key, value) in environmentValues)
        {
            originalEnvironment[key] = Environment.GetEnvironmentVariable(key);
            Environment.SetEnvironmentVariable(key, value);
        }
    }

    private void RestoreEnvironment()
    {
        foreach (var (key, value) in originalEnvironment)
        {
            Environment.SetEnvironmentVariable(key, value);
        }
    }

    private static string Sha256Hex(string value)
    {
        return Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(value)));
    }
}
