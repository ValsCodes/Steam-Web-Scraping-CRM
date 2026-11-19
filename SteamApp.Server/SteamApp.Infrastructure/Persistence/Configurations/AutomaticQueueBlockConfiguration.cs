using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SteamApp.Domain.Entities;

namespace SteamApp.Infrastructure.Persistence.Configurations;

internal sealed class AutomaticQueueBlockConfiguration : IEntityTypeConfiguration<AutomaticQueueBlock>
{
    public void Configure(EntityTypeBuilder<AutomaticQueueBlock> entity)
    {
        entity.ToTable("automatic_queue_block");
        entity.HasKey(x => x.Id);
        entity.Property(x => x.Id).HasColumnName("id");
        entity.Property(x => x.AutomaticQueueDefinitionId).HasColumnName("automatic_queue_definition_id");
        entity.Property(x => x.BlockKey).HasColumnName("block_key");
        entity.Property(x => x.SortOrder).HasColumnName("sort_order");
        entity.Property(x => x.BlockType).HasColumnName("block_type");
        entity.Property(x => x.ConfigurationJson).HasColumnName("configuration_json");
    }
}
