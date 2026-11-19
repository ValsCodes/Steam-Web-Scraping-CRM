using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SteamApp.Domain.Entities;

namespace SteamApp.Infrastructure.Persistence.Configurations;

internal sealed class AutomaticQueueRunConfiguration : IEntityTypeConfiguration<AutomaticQueueRun>
{
    public void Configure(EntityTypeBuilder<AutomaticQueueRun> entity)
    {
        entity.ToTable("automatic_queue_run");
        entity.HasKey(x => x.Id);
        entity.Property(x => x.Id).HasColumnName("id");
        entity.Property(x => x.AutomaticQueueDefinitionId).HasColumnName("automatic_queue_definition_id");
        entity.Property(x => x.UserId).HasColumnName("user_id");
        entity.Property(x => x.QueueName).HasColumnName("queue_name");
        entity.Property(x => x.Status).HasColumnName("status");
        entity.Property(x => x.PauseReason).HasColumnName("pause_reason");
        entity.Property(x => x.CurrentBlockIndex).HasColumnName("current_block_index");
        entity.Property(x => x.Date).HasColumnName("date");
        entity.Property(x => x.StartedAtUtc).HasColumnName("started_at_utc");
        entity.Property(x => x.CompletedAtUtc).HasColumnName("completed_at_utc");
        entity.Property(x => x.ErrorText).HasColumnName("error_text");
        entity.Property(x => x.CorrelationId).HasColumnName("correlation_id");
    }
}
