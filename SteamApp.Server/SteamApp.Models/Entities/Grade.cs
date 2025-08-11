using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace SteamApp.Models.Entities;

[Table("grade")]
public class Grade : BaseModel
{
    [JsonIgnore]
    public virtual ICollection<ItemSkins> ItemSkins { get; set; } = [];
}
