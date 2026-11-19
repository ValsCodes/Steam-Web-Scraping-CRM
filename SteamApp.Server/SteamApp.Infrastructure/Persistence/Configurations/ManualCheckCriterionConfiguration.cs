using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SteamApp.Domain.Entities;

namespace SteamApp.Infrastructure.Persistence.Configurations;

internal sealed class ManualCheckCriterionConfiguration : IEntityTypeConfiguration<ManualCheckCriterion>
{
    public void Configure(EntityTypeBuilder<ManualCheckCriterion> entity)
    {
        entity.HasKey(x => x.Id);
        entity.Property(x => x.Id).HasColumnName("id");
        entity.Property(x => x.ManualCheckPresetId).HasColumnName("manual_check_preset_id");
        entity.Property(x => x.ConditionOperatorId).HasColumnName("condition_operator_id");
        entity.Property(x => x.SortOrder).HasColumnName("sort_order");
        entity.Property(x => x.OpenGroupCount).HasColumnName("open_group_count");
        entity.Property(x => x.CloseGroupCount).HasColumnName("close_group_count");
        entity.Property(x => x.NameContains).HasColumnName("name_contains");
        entity.Property(x => x.ValueContains).HasColumnName("value_contains");
    }
}
