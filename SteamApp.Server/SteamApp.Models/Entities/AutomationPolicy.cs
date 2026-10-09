using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SteamApp.Domain.Entities;

[Table("automation_policy")]
public sealed class AutomationPolicy
{
    public const int SingletonId = 1;
    public const int DefaultNonAdminLimitSeconds = 900;
    public const int MaximumNonAdminLimitSeconds = 24 * 60 * 60;

    [Key]
    [Column("id")]
    public int Id { get; set; } = SingletonId;

    [Column("non_admin_limit_seconds")]
    public int NonAdminLimitSeconds { get; set; } = DefaultNonAdminLimitSeconds;

    [Column("usage_reset_at_utc")]
    public DateTime? UsageResetAtUtc { get; set; }

    [MaxLength(450)]
    [Column("last_modified_by_user_id")]
    public string? LastModifiedByUserId { get; set; }

    [Column("last_modified_at_utc")]
    public DateTime LastModifiedAtUtc { get; set; }

    [MaxLength(450)]
    [Column("last_reset_by_user_id")]
    public string? LastResetByUserId { get; set; }

    [Column("last_reset_at_utc")]
    public DateTime? LastResetAtUtc { get; set; }

    [Timestamp]
    [Column("row_version")]
    public byte[] RowVersion { get; set; } = [];
}
