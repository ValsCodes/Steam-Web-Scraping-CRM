namespace SteamApp.Domain.ValueObjects.Authentication;

public class UserProfileResponse
{
    public string Id { get; set; } = null!;
    public string? FirstName { get; set; }
    public string? LastName { get; set; }
    public string? UserName { get; set; }
    public string? Email { get; set; }
    public string? Phone { get; set; }
    public string DisplayName { get; set; } = null!;
}
