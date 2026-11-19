using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SteamApp.Domain.Entities;

namespace SteamApp.Infrastructure.Persistence.Configurations;

internal sealed class GameUrlPixelsConfiguration : IEntityTypeConfiguration<GameUrlPixels>
{
    public void Configure(EntityTypeBuilder<GameUrlPixels> entity)
    {
        entity.ToTable("game_url_pixels");
        entity.Property(x => x.GameUrlId).HasColumnName("game_url_id");
        entity.Property(x => x.PixelId).HasColumnName("pixel_id");
    }
}
