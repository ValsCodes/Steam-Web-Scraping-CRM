using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SteamApp.Domain.Entities;

namespace SteamApp.Infrastructure.Persistence.Configurations;

internal sealed class GameUrlProductStockHistoryConfiguration : IEntityTypeConfiguration<GameUrlProductStockHistory>
{
    public void Configure(EntityTypeBuilder<GameUrlProductStockHistory> entity)
    {
        entity.ToTable("game_url_product_stock_history");
        entity.HasKey(x => x.Id);
        entity.Property(x => x.Id).HasColumnName("id");
        entity.Property(x => x.ProductId).HasColumnName("product_id");
        entity.Property(x => x.GameUrlId).HasColumnName("game_url_id");
        entity.Property(x => x.PreviousStock).HasColumnName("previous_stock");
        entity.Property(x => x.NewStock).HasColumnName("new_stock");
        entity.Property(x => x.Operation).HasColumnName("operation");
        entity.Property(x => x.CreatedAtUtc).HasColumnName("created_at_utc");
        entity.Property(x => x.UserId).HasColumnName("user_id");
    }
}
