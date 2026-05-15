using System.Net;
using SteamApp.IntegrationTests.Support;

namespace SteamApp.E2ETests.Security;

[TestFixture]
public sealed class StartupConfigurationE2ETests
{
    [Test]
    public async Task SwaggerIsExposedInDevelopmentAndHiddenBehindProductionApiSecurity()
    {
        using var devFactory = new SteamAppFactory(environmentName: "Development");
        using var prodFactory = new SteamAppFactory(environmentName: "Production");
        using var devClient = devFactory.CreateAnonymousClient();
        using var prodClient = prodFactory.CreateAuthenticatedClient();

        var devSwagger = await devClient.GetAsync("/swagger/v1/swagger.json");
        var prodSwagger = await prodClient.GetAsync("/swagger/v1/swagger.json");

        Assert.Multiple(() =>
        {
            Assert.That(devSwagger.StatusCode, Is.EqualTo(HttpStatusCode.OK));
            Assert.That(prodSwagger.StatusCode, Is.EqualTo(HttpStatusCode.NotFound));
        });
    }

    [Test]
    public void ProductionStartupRejectsWeakJwtSigningKeys()
    {
        using var factory = new SteamAppFactory(
            environmentName: "Production",
            overrides: new Dictionary<string, string?>
            {
                ["JwtSettings__Key"] = "too-short"
            });

        var exception = Assert.Throws<InvalidOperationException>(
            () => factory.CreateAnonymousClient());

        Assert.That(
            exception?.Message,
            Does.Contain("JwtSettings:Key must be at least 32 bytes"));
    }
}
