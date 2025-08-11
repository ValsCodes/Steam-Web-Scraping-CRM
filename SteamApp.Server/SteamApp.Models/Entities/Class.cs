using System.ComponentModel.DataAnnotations.Schema;
using System.Runtime.Serialization;
using System.Text.Json.Serialization;

namespace SteamApp.Models.Entities;

[Table("class")]
public class Class : BaseModel
{
    [JsonIgnore]
    public virtual ICollection<Item> Items { get; set; } = [];
}
