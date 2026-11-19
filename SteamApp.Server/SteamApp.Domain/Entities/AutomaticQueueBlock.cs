using System.ComponentModel.DataAnnotations;
using SteamApp.Domain.Enums;

namespace SteamApp.Domain.Entities;

public sealed class AutomaticQueueBlock
{
    public long Id { get; set; }

    public long AutomaticQueueDefinitionId { get; set; }

    public AutomaticQueueDefinition AutomaticQueueDefinition { get; set; } = null!;

    public Guid BlockKey { get; set; }

    public int SortOrder { get; set; }

    public AutomaticQueueBlockTypeEnum BlockType { get; set; }

    [Required]
    public string ConfigurationJson { get; set; } = "{}";
}
