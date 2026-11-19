using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SteamApp.Domain.Entities;

namespace SteamApp.Infrastructure.Persistence.Configurations;

internal sealed class FeedbackRequestConfiguration : IEntityTypeConfiguration<FeedbackRequest>
{
    public void Configure(EntityTypeBuilder<FeedbackRequest> entity)
    {
        entity.ToTable("feedback_request");
        entity.HasKey(x => x.Id);
        entity.Property(x => x.Id).HasColumnName("id");
        entity.Property(x => x.Type).HasColumnName("type");
        entity.Property(x => x.Title).HasColumnName("title");
        entity.Property(x => x.Description).HasColumnName("description");
        entity.Property(x => x.Area).HasColumnName("area");
        entity.Property(x => x.TargetResourceType).HasColumnName("target_resource_type");
        entity.Property(x => x.TargetResourceId).HasColumnName("target_resource_id");
        entity.Property(x => x.Status).HasColumnName("status");
        entity.Property(x => x.CreatedAtUtc).HasColumnName("created_at_utc");
        entity.Property(x => x.UpdatedAtUtc).HasColumnName("updated_at_utc");
        entity.Property(x => x.StatusChangedAtUtc).HasColumnName("status_changed_at_utc");
        entity.Property(x => x.UserId).HasColumnName("user_id");
    }
}
