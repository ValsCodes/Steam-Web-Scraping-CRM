using System.Net;
using SteamApp.IntegrationTests.Support;

namespace SteamApp.IntegrationTests.Observability;

[TestFixture]
public sealed class ObservabilityIntegrationTests
{
    [Test]
    public async Task CollectorUnavailable_DoesNotPreventStartupOrRequests()
    {
        var configuration = new Dictionary<string, string?>
        {
            ["Observability__Enabled"] = "true",
            ["OTEL_SERVICE_NAME"] = "steamapp-integration-tests",
            ["OTEL_EXPORTER_OTLP_ENDPOINT"] = "http://127.0.0.1:1",
            ["OTEL_EXPORTER_OTLP_PROTOCOL"] = "http/protobuf",
            ["OTEL_METRIC_EXPORT_INTERVAL"] = "100"
        };

        using var factory = new SteamAppFactory(overrides: configuration);
        using var client = factory.CreateAnonymousClient();

        using var response = await client.GetAsync("/api/games/");

        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.Unauthorized));
    }
}
