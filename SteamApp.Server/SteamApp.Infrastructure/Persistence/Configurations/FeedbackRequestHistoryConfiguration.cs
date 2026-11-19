using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SteamApp.Domain.Entities;

namespace SteamApp.Infrastructure.Persistence.Configurations;

internal sealed class FeedbackRequestHistoryConfiguration : IEntityTypeConfiguration<FeedbackRequestHistory>
{
    public void Configure(EntityTypeBuilder<FeedbackRequestHistory> entity)
    {
        entity.ToTable("feedback_request_history");
        entity.HasKey(x => x.Id);
        entity.Property(x => x.Id).HasColumnName("id");
        entity.Property(x => x.FeedbackRequestId).HasColumnName("feedback_request_id");
        entity.Property(x => x.Action).HasColumnName("action");
        entity.Property(x => x.CreatedAtUtc).HasColumnName("created_at_utc");
        entity.Property(x => x.PreviousType).HasColumnName("previous_type");
        entity.Property(x => x.NewType).HasColumnName("new_type");
        entity.Property(x => x.PreviousTitle).HasColumnName("previous_title");
        entity.Property(x => x.NewTitle).HasColumnName("new_title");
        entity.Property(x => x.PreviousDescription).HasColumnName("previous_description");
        entity.Property(x => x.NewDescription).HasColumnName("new_description");
        entity.Property(x => x.PreviousArea).HasColumnName("previous_area");
        entity.Property(x => x.NewArea).HasColumnName("new_area");
        entity.Property(x => x.PreviousStatus).HasColumnName("previous_status");
        entity.Property(x => x.NewStatus).HasColumnName("new_status");
        entity.Property(x => x.UserId).HasColumnName("user_id");
    }
}
