using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace SteamApp.Models.Entities;

[Table("slot")]
public class Slot : BaseModel
{
    [JsonIgnore]
    public virtual ICollection<Item> Items { get; set; } = [];
}
