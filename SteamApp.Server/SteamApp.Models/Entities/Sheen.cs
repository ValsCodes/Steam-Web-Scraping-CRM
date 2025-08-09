using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace SteamApp.Models.Entities;

[Table("sheen")]
public class Sheen
{
    [Column("id")]
    [Key]
    public short Id { get; set; }

    [Column("name")]
    public string Name { get; set; }

    [Column("is_good_sheen")]
    public bool IsGoodSheen{ get; set; }

    [JsonIgnore]
    public virtual ICollection<WatchItem> Products { get; set; } = [];
}
