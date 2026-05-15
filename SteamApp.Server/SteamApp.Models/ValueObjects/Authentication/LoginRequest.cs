using System.ComponentModel.DataAnnotations;

namespace SteamApp.Domain.ValueObjects.Authentication;

public class LoginRequest
{
    [Required]
    public string EmailOrUserName { get; set; } = null!;

    [Required]
    public string Password { get; set; } = null!;
}
