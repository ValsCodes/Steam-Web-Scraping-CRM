using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SteamApp.Domain.Entities;

namespace SteamApp.Infrastructure.Persistence.Configurations;

internal sealed class ManualCheckPresetGameUrlConfiguration : IEntityTypeConfiguration<ManualCheckPresetGameUrl>
{
    public void Configure(EntityTypeBuilder<ManualCheckPresetGameUrl> entity)
    {
        entity.ToTable("manual_check_preset_game_url");
        entity.Property(x => x.ManualCheckPresetId).HasColumnName("manual_check_preset_id");
        entity.Property(x => x.GameUrlId).HasColumnName("game_url_id");
    }
}
