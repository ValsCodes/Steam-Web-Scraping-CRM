using System.ComponentModel.DataAnnotations;

namespace SteamApp.Domain.Entities;

public sealed class AutomationPolicy
{
    public const int SingletonId = 1;
    public const int DefaultNonAdminLimitSeconds = 900;
    public const int MaximumNonAdminLimitSeconds = 24 * 60 * 60;

    public int Id { get; set; } = SingletonId;

    public int NonAdminLimitSeconds { get; set; } = DefaultNonAdminLimitSeconds;

    public DateTime? UsageResetAtUtc { get; set; }

    [MaxLength(450)]
    public string? LastModifiedByUserId { get; set; }

    public DateTime LastModifiedAtUtc { get; set; }

    [MaxLength(450)]
    public string? LastResetByUserId { get; set; }

    public DateTime? LastResetAtUtc { get; set; }

    public byte[] RowVersion { get; set; } = [];
}
