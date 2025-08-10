using SteamApp.Models.Entities.ManyToMany;
using System.ComponentModel.DataAnnotations.Schema;

namespace SteamApp.Models.Entities;

[Table("slot")]
public class Slot : BaseModel
{
    public ICollection<ItemSlots> ItemSlots { get; set; } = [];
}
