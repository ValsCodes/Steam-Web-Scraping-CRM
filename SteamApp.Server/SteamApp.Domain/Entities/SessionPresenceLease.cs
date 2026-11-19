using System.ComponentModel.DataAnnotations;

namespace SteamApp.Domain.Entities;

public sealed class SessionPresenceLease
{
    [Required]
    [MaxLength(450)]
    public string UserId { get; set; } = string.Empty;

    public Guid TabId { get; set; }

    public DateTime UpdatedAtUtc { get; set; }

    public DateTime ExpiresAtUtc { get; set; }
}
