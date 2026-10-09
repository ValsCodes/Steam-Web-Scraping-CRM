using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SteamApp.Domain.Entities;

[Table("session_presence_lease")]
public sealed class SessionPresenceLease
{
    [Required]
    [MaxLength(450)]
    [Column("user_id")]
    public string UserId { get; set; } = string.Empty;

    [Column("tab_id")]
    public Guid TabId { get; set; }

    [Column("updated_at_utc")]
    public DateTime UpdatedAtUtc { get; set; }

    [Column("expires_at_utc")]
    public DateTime ExpiresAtUtc { get; set; }
}
