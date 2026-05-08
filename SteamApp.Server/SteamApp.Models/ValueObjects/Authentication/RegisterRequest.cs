using System.ComponentModel.DataAnnotations;

namespace SteamApp.Domain.ValueObjects.Authentication;

public class RegisterRequest
{
    [Required]
    [EmailAddress]
    public string Email { get; set; } = null!;

    public string? UserName { get; set; }

    [Required]
    [MinLength(8)]
    public string Password { get; set; } = null!;
}
