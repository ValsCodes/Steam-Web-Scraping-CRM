using System.Net;
using SteamApp.IntegrationTests.Support;

namespace SteamApp.IntegrationTests.Startup;

[TestFixture]
public sealed class StartupIntegrationTests
{
    [Test]
    public async Task AppStartsWithValidProductionLikeConfiguration()
    {
        using var factory = new SteamAppFactory(environmentName: "Production");
        using var client = factory.CreateAnonymousClient();
        await factory.ResetDatabaseAsync();

        var response = await client.GetAsync("/api/games/");

        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.Unauthorized));
    }

    [Test]
    public void AppFailsStartupWhenRequiredConfigurationIsMissing()
    {
        using var factory = new SteamAppFactory(
            environmentName: "Production",
            applyDefaultConfiguration: false,
            overrides: ClearRequiredConfiguration());

        Assert.That(
            () => factory.CreateAnonymousClient(),
            Throws.InvalidOperationException.With.Message.Contains("Missing required configuration"));
    }

    [Test]
    public void AppFailsStartupWithWeakJwtKey()
    {
        using var factory = new SteamAppFactory(
            environmentName: "Production",
            overrides: new Dictionary<string, string?>
            {
                ["JwtSettings__Key"] = "short"
            });

        Assert.That(
            () => factory.CreateAnonymousClient(),
            Throws.InvalidOperationException.With.Message.Contains("at least 32 bytes"));
    }

    [Test]
    public void ProductionAppFailsStartupWithInvalidCorsOrigin()
    {
        using var factory = new SteamAppFactory(
            environmentName: "Production",
            overrides: new Dictionary<string, string?>
            {
                ["Cors__AllowedOrigins__0"] = "http://spa.example.test"
            });

        Assert.That(
            () => factory.CreateAnonymousClient(),
            Throws.InvalidOperationException.With.Message.Contains("must use HTTPS"));
    }

    [Test]
    public void ProductionAppFailsStartupWithWildcardAllowedHosts()
    {
        using var factory = new SteamAppFactory(
            environmentName: "Production",
            overrides: new Dictionary<string, string?>
            {
                ["AllowedHosts"] = "*"
            });

        Assert.That(
            () => factory.CreateAnonymousClient(),
            Throws.InvalidOperationException.With.Message.Contains("AllowedHosts"));
    }

    private static IReadOnlyDictionary<string, string?> ClearRequiredConfiguration()
    {
        return new Dictionary<string, string?>
        {
            ["ConnectionStrings__DefaultConnection"] = null,
            ["JwtSettings__Key"] = null,
            ["JwtSettings__Issuer"] = null,
            ["JwtSettings__Audience"] = null,
            ["JwtSettings__DurationMinutes"] = null,
            ["Clients__0__ClientId"] = null,
            ["Clients__0__ClientSecretHash"] = null,
            ["Clients__0__AllowedScope"] = null
        };
    }
}
