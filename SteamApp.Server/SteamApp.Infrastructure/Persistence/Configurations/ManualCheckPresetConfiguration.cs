using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SteamApp.Domain.Entities;

namespace SteamApp.Infrastructure.Persistence.Configurations;

internal sealed class ManualCheckPresetConfiguration : IEntityTypeConfiguration<ManualCheckPreset>
{
    public void Configure(EntityTypeBuilder<ManualCheckPreset> entity)
    {
        entity.ToTable("manual_check_preset");
        entity.HasKey(x => x.Id);
        entity.Property(x => x.Id).HasColumnName("id");
        entity.Property(x => x.UserId).HasColumnName("user_id");
        entity.Property(x => x.GameId).HasColumnName("game_id");
        entity.Property(x => x.ItemGroupId).HasColumnName("item_group_id");
        entity.Property(x => x.Name).HasColumnName("name");
        entity.Property(x => x.ListingLimit).HasColumnName("listing_limit");
        entity.Property(x => x.PriceRangeMode).HasColumnName("price_range_mode");
        entity.Property(x => x.MinimumPriceMinorUnits).HasColumnName("minimum_price_minor_units");
        entity.Property(x => x.MaximumPriceMinorUnits).HasColumnName("maximum_price_minor_units");
        entity.Property(x => x.CooldownMinutes).HasColumnName("cooldown_minutes");
        entity.Property(x => x.CooldownSeconds).HasColumnName("cooldown_seconds");
        entity.Property(x => x.CreatedAtUtc).HasColumnName("created_at_utc");
        entity.Property(x => x.UpdatedAtUtc).HasColumnName("updated_at_utc");
    }
}
