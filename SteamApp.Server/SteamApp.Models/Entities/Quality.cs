using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace SteamApp.Models.Entities;

[Table("quality")]
public class Quality
{
    [Column("id")]
    [Key]
    public short Id { get; set; }

    [Column("name")]
    public string Name { get; set; }

    [JsonIgnore]
    public virtual ICollection<WatchItem> Products { get; set; } = [];

    [JsonIgnore]
    public virtual ICollection<Skin> Skins { get; set; } = [];
}
