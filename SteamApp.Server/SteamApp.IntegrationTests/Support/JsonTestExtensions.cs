using System.Text.Json;

namespace SteamApp.IntegrationTests.Support;

public static class JsonTestExtensions
{
    public static async Task<JsonElement> ReadJsonElementAsync(this HttpResponseMessage response)
    {
        await using var stream = await response.Content.ReadAsStreamAsync();
        using var document = await JsonDocument.ParseAsync(stream);
        return document.RootElement.Clone();
    }
}
