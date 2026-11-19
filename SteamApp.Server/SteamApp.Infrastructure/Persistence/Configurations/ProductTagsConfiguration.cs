using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SteamApp.Domain.Entities;

namespace SteamApp.Infrastructure.Persistence.Configurations;

internal sealed class ProductTagsConfiguration : IEntityTypeConfiguration<ProductTags>
{
    public void Configure(EntityTypeBuilder<ProductTags> entity)
    {
        entity.ToTable("product_tags");
        entity.Property(x => x.TagId).HasColumnName("tag_id");
        entity.Property(x => x.ProductId).HasColumnName("product_id");
    }
}
