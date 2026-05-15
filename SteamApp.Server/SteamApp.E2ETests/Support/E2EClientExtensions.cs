using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using SteamApp.IntegrationTests.Support;

namespace SteamApp.E2ETests.Support;

internal static class E2EClientExtensions
{
    public static async Task AuthenticateWithClientCredentialsAsync(this HttpClient client)
    {
        var tokenResponse = await client.PostAsJsonAsync("/api/auth/token", new
        {
            clientId = "integration-client",
            clientSecret = "integration-secret"
        });

        tokenResponse.EnsureSuccessStatusCode();

        var tokenJson = await tokenResponse.ReadJsonElementAsync();
        client.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue(
                "Bearer",
                tokenJson.GetProperty("token").GetString());
    }

    public static async Task<JsonElement> ReadRequiredJsonAsync(this HttpResponseMessage response)
    {
        response.EnsureSuccessStatusCode();
        return await response.ReadJsonElementAsync();
    }
}
