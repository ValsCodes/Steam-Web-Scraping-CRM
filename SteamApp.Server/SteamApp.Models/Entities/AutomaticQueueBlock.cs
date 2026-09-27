using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using SteamApp.Domain.Enums;

namespace SteamApp.Domain.Entities;

[Table("automatic_queue_block")]
public sealed class AutomaticQueueBlock
{
    [Key]
    [Column("id")]
    public long Id { get; set; }

    [Column("automatic_queue_definition_id")]
    public long AutomaticQueueDefinitionId { get; set; }

    public AutomaticQueueDefinition AutomaticQueueDefinition { get; set; } = null!;

    [Column("block_key")]
    public Guid BlockKey { get; set; }

    [Column("sort_order")]
    public int SortOrder { get; set; }

    [Column("block_type")]
    public AutomaticQueueBlockTypeEnum BlockType { get; set; }

    [Required]
    [Column("configuration_json")]
    public string ConfigurationJson { get; set; } = "{}";
}
