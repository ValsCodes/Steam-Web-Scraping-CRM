using System.ComponentModel.DataAnnotations;

namespace SteamApp.Domain.ValueObjects.Authentication;

public class RegisterRequest
{
    [StringLength(100)]
    public string? FirstName { get; set; }

    [StringLength(100)]
    public string? LastName { get; set; }

    [Required]
    [EmailAddress]
    public string Email { get; set; } = null!;

    [Phone]
    public string? Phone { get; set; }

    public string? UserName { get; set; }

    [Required]
    [MinLength(8)]
    public string Password { get; set; } = null!;
}
