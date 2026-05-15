using System.ComponentModel.DataAnnotations;

namespace SteamApp.Domain.ValueObjects.Authentication;

public class ChangePasswordRequest
{
    [Required]
    public string CurrentPassword { get; set; } = null!;

    [Required]
    [MinLength(8)]
    public string NewPassword { get; set; } = null!;
}
