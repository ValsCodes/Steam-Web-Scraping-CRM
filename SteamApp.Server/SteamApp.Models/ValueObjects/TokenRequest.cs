namespace SteamApp.Models.ValueObjects;

public class TokenRequest
{
    public string ClientId { get; set; } = null!;
    public string ClientSecret { get; set; } = null!;
}
