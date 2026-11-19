using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SteamApp.Domain.Entities;

namespace SteamApp.Infrastructure.Persistence.Configurations;

internal sealed class AutomaticQueueRunBlockConfiguration : IEntityTypeConfiguration<AutomaticQueueRunBlock>
{
    public void Configure(EntityTypeBuilder<AutomaticQueueRunBlock> entity)
    {
        entity.ToTable("automatic_queue_run_block");
        entity.HasKey(x => x.Id);
        entity.Property(x => x.Id).HasColumnName("id");
        entity.Property(x => x.AutomaticQueueRunId).HasColumnName("automatic_queue_run_id");
        entity.Property(x => x.BlockKey).HasColumnName("block_key");
        entity.Property(x => x.SortOrder).HasColumnName("sort_order");
        entity.Property(x => x.BlockType).HasColumnName("block_type");
        entity.Property(x => x.Status).HasColumnName("status");
        entity.Property(x => x.SetupJson).HasColumnName("setup_json");
        entity.Property(x => x.ManualCheckRunId).HasColumnName("manual_check_run_id");
        entity.Property(x => x.WaitUntilUtc).HasColumnName("wait_until_utc");
        entity.Property(x => x.RemainingDelaySeconds).HasColumnName("remaining_delay_seconds");
        entity.Property(x => x.StartedAtUtc).HasColumnName("started_at_utc");
        entity.Property(x => x.CompletedAtUtc).HasColumnName("completed_at_utc");
        entity.Property(x => x.ErrorText).HasColumnName("error_text");
    }
}
