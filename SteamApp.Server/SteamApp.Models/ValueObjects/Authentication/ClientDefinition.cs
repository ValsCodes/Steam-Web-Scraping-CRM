namespace SteamApp.Domain.ValueObjects.Authentication;

public class ClientDefinition
{
    public string ClientId { get; set; } = null!;
    public string? ClientSecret { get; set; }
    public string? ClientSecretHash { get; set; }
    public string AllowedScope { get; set; } = null!;
}
