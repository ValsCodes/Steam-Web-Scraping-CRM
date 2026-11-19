using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SteamApp.Domain.Entities;

namespace SteamApp.Infrastructure.Persistence.Configurations;

internal sealed class GameUrlConfiguration : IEntityTypeConfiguration<GameUrl>
{
    public void Configure(EntityTypeBuilder<GameUrl> entity)
    {
        entity.ToTable("game_url");
        entity.HasKey(x => x.Id);
        entity.Property(x => x.Id).HasColumnName("id");
        entity.Property(x => x.Name).HasColumnName("name");
        entity.Property(x => x.GameId).HasColumnName("game_id");
        entity.Property(x => x.ItemGroupId).HasColumnName("item_group_id");
        entity.Property(x => x.ScrapingModeId).HasColumnName("scraping_mode_id");
        entity.Property(x => x.PartialUrl).HasColumnName("partial_url");
        entity.Property(x => x.StartPage).HasColumnName("start_page");
        entity.Property(x => x.EndPage).HasColumnName("end_page");
        entity.Property(x => x.PixelX).HasColumnName("pixel_x");
        entity.Property(x => x.PixelY).HasColumnName("pixel_y");
        entity.Property(x => x.PixelImageWidth).HasColumnName("pixel_image_width");
        entity.Property(x => x.PixelImageHeight).HasColumnName("pixel_image_height");
        entity.Property(x => x.IsActive).HasColumnName("is_active");
        entity.Property(x => x.UserId).HasColumnName("user_id");

        entity.HasOne(x => x.Game)
              .WithMany(x => x.GameUrls)
              .HasForeignKey(x => x.GameId);
    }
}
