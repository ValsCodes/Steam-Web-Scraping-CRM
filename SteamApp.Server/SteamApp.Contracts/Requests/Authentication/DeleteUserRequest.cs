using System.ComponentModel.DataAnnotations;

namespace SteamApp.Contracts.Authentication;

public class DeleteUserRequest
{
    [Required]
    public string Password { get; set; } = null!;
}
