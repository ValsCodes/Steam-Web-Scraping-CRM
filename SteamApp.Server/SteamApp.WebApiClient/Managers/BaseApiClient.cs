using System.IdentityModel.Tokens.Jwt;
using System.Net.Http.Headers;
using System.Net.Http.Json;

namespace SteamApp.WebApiClient.Managers;

public class BaseApiClient(HttpClient http, AuthApiClient auth)
{
    private string? _token;
    private DateTime _expiresAt;

    private async Task EnsureTokenAsync(CancellationToken ct)
    {
        if (_token != null && _expiresAt > DateTime.UtcNow.AddMinutes(1))
        {
            return;
        }

        _token = await auth.GetTokenAsync(ct);

        var jwt = new JwtSecurityTokenHandler().ReadJwtToken(_token);
        _expiresAt = jwt.ValidTo;

        http.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", _token);
    }

    public async Task<T?> GetAsync<T>(string url, CancellationToken ct)
    {
        await EnsureTokenAsync(ct);
        return await http.GetFromJsonAsync<T>(url, ct);
    }

    public async Task<T> PostAsync<T>(string url, object body, CancellationToken ct)
    {
        await EnsureTokenAsync(ct);

        var res = await http.PostAsJsonAsync(url, body, ct);
        res.EnsureSuccessStatusCode();

        return (await res.Content.ReadFromJsonAsync<T>(ct))!;
    }
}
