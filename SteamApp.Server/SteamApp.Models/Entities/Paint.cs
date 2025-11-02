using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace SteamApp.Models.Entities;
[Table("paint")]
public class Paint
{
    [Column("id")]
    [Key]
    public short Id { get; set; }

    [Column("name")]
    public string Name { get; set; }

    [Column("r_value")]
    public byte R { get; set; }

    [Column("g_value")]
    public byte G { get; set; }

    [Column("b_value")]
    public byte B { get; set; }

    [Column("is_good_paint")]
    public bool IsGoodPaint { get; set; }

    [JsonIgnore]
    public virtual ICollection<Item> Items { get; set; } = [];
}
