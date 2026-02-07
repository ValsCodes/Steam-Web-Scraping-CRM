namespace SteamApp.WebAPI.Utilities;

public static class HttpUtilities
{
    public static async Task<string> GetHttpResposeAsync(string url, CancellationToken ct = default)
    {
        using HttpClient httpClient = new();
        httpClient.Timeout = TimeSpan.FromSeconds(30);

        var response = await httpClient.GetAsync(url, ct).ConfigureAwait(false);

        // response.EnsureSuccessStatusCode();

        string responseContent = await response.Content.ReadAsStringAsync(ct);

        if (responseContent == null)
        {
            return null!;
        }

        return responseContent;
    }
}
