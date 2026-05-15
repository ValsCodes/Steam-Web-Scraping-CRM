using System.ComponentModel.DataAnnotations;

namespace SteamApp.Domain.ValueObjects.Authentication;

public class DeleteUserRequest
{
    [Required]
    public string Password { get; set; } = null!;
}
