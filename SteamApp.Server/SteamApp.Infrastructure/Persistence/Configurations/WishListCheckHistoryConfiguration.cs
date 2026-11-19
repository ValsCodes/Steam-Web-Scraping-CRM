using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SteamApp.Domain.Entities;

namespace SteamApp.Infrastructure.Persistence.Configurations;

internal sealed class WishListCheckHistoryConfiguration : IEntityTypeConfiguration<WishListCheckHistory>
{
    public void Configure(EntityTypeBuilder<WishListCheckHistory> entity)
    {
        entity.ToTable("wish_list_check_history");
        entity.HasKey(x => x.Id);
        entity.Property(x => x.Id).HasColumnName("id");
        entity.Property(x => x.WishListId).HasColumnName("wish_list_id");
        entity.Property(x => x.UserId).HasColumnName("user_id");
        entity.Property(x => x.GameName).HasColumnName("game_name");
        entity.Property(x => x.Source).HasColumnName("source");
        entity.Property(x => x.Status).HasColumnName("status");
        entity.Property(x => x.TargetPrice).HasColumnName("target_price");
        entity.Property(x => x.CurrentPrice).HasColumnName("current_price");
        entity.Property(x => x.IsPriceReached).HasColumnName("is_price_reached");
        entity.Property(x => x.RequestedAtUtc).HasColumnName("requested_at_utc");
        entity.Property(x => x.StartedAtUtc).HasColumnName("started_at_utc");
        entity.Property(x => x.CompletedAtUtc).HasColumnName("completed_at_utc");
        entity.Property(x => x.CorrelationId).HasColumnName("correlation_id");
        entity.Property(x => x.ErrorCode).HasColumnName("error_code");
        entity.Property(x => x.ErrorText).HasColumnName("error_text");
    }
}
