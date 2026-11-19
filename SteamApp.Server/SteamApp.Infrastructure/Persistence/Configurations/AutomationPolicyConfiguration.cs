using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SteamApp.Domain.Entities;

namespace SteamApp.Infrastructure.Persistence.Configurations;

internal sealed class AutomationPolicyConfiguration : IEntityTypeConfiguration<AutomationPolicy>
{
    public void Configure(EntityTypeBuilder<AutomationPolicy> entity)
    {
        entity.HasKey(x => x.Id);
        entity.Property(x => x.Id).HasColumnName("id");
        entity.Property(x => x.NonAdminLimitSeconds).HasColumnName("non_admin_limit_seconds");
        entity.Property(x => x.UsageResetAtUtc).HasColumnName("usage_reset_at_utc");
        entity.Property(x => x.LastModifiedByUserId).HasColumnName("last_modified_by_user_id");
        entity.Property(x => x.LastModifiedAtUtc).HasColumnName("last_modified_at_utc");
        entity.Property(x => x.LastResetByUserId).HasColumnName("last_reset_by_user_id");
        entity.Property(x => x.LastResetAtUtc).HasColumnName("last_reset_at_utc");
        entity.Property(x => x.RowVersion).HasColumnName("row_version");
    }
}
