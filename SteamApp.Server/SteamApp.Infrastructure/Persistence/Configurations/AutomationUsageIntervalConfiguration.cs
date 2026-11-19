using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SteamApp.Domain.Entities;

namespace SteamApp.Infrastructure.Persistence.Configurations;

internal sealed class AutomationUsageIntervalConfiguration : IEntityTypeConfiguration<AutomationUsageInterval>
{
    public void Configure(EntityTypeBuilder<AutomationUsageInterval> entity)
    {
        entity.ToTable("automation_usage_interval");
        entity.HasKey(x => x.Id);
        entity.Property(x => x.Id).HasColumnName("id");
        entity.Property(x => x.UserId).HasColumnName("user_id");
        entity.Property(x => x.ManualCheckRunId).HasColumnName("manual_check_run_id");
        entity.Property(x => x.StartedAtUtc).HasColumnName("started_at_utc");
        entity.Property(x => x.EndedAtUtc).HasColumnName("ended_at_utc");
    }
}
