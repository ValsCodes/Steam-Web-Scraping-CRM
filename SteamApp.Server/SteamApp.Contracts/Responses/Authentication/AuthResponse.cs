namespace SteamApp.Contracts.Authentication;

public class AuthResponse
{
    public string Token { get; set; } = null!;
    public string TokenType { get; set; } = "Bearer";
    public DateTime ExpiresAtUtc { get; set; }
}
