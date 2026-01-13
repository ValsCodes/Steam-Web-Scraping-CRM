using System.Net.Http.Json;
using Microsoft.Extensions.Configuration;

namespace SteamApp.Infrastructure;

public sealed class AuthApiClient(HttpClient http, IConfiguration config)
{

    public async Task<string> GetTokenAsync(CancellationToken ct)
    {
        var payload = new
        {
            ClientId = config["InternalClient:ClientId"],
            ClientSecret = config["InternalClient:ClientSecret"]
        };

        var res = await http.PostAsJsonAsync("api/auth/token", payload, ct);
        res.EnsureSuccessStatusCode();

        var body = await res.Content.ReadFromJsonAsync<TokenResponse>(ct);
        return body!.Token;
    }

    private sealed class TokenResponse
    {
        public string Token { get; set; } = null!;
    }
}
