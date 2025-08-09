using SteamApp.Models.Entities.OneToOne;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace SteamApp.Models.Entities;

[Table("class")]
public class Class : BaseModel
{
    [JsonIgnore]
    public virtual ICollection<Item> Items { get; set; } = [];

    public virtual ICollection<TeamFotressItem> TeamFotressItems { get; set; } = [];
}
