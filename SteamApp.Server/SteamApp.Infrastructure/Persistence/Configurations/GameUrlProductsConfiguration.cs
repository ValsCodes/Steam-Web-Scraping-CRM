using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SteamApp.Domain.Entities;

namespace SteamApp.Infrastructure.Persistence.Configurations;

internal sealed class GameUrlProductsConfiguration : IEntityTypeConfiguration<GameUrlProducts>
{
    public void Configure(EntityTypeBuilder<GameUrlProducts> entity)
    {
        entity.ToTable("game_url_products");
        entity.Property(x => x.GameUrlId).HasColumnName("game_url_id");
        entity.Property(x => x.ProductId).HasColumnName("product_id");
        entity.Property(x => x.CurrentStock).HasColumnName("current_stock");
    }
}
