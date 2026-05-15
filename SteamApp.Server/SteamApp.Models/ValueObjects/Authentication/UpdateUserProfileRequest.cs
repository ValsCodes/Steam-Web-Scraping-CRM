using System.ComponentModel.DataAnnotations;

namespace SteamApp.Domain.ValueObjects.Authentication;

public class UpdateUserProfileRequest
{
    [StringLength(100)]
    public string? FirstName { get; set; }

    [StringLength(100)]
    public string? LastName { get; set; }

    public string? UserName { get; set; }

    [Required]
    [EmailAddress]
    public string Email { get; set; } = null!;

    [Phone]
    public string? Phone { get; set; }
}
