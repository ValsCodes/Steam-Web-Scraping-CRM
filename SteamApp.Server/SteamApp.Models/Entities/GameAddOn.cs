using SteamApp.Models.Entities.ManyToMany;
using SteamApp.Models.Entities.OneToOne;
using System.ComponentModel.DataAnnotations.Schema;

namespace SteamApp.Models.Entities
{
    [Table("game_add_on")]
    public class GameAddOn : BaseModel
    {
        [Column("game_id")]
        public long GameId { get; set; }

        [ForeignKey(nameof(GameId))]
        public Game Game { get; set; }

        [Column("add_on_type_id")]
        public short AddOnTypeId { get; set; }

        [ForeignKey(nameof(AddOnTypeId))]
        public AddOnType AddOnType { get; set; }

        [Column("added_value")]
        public decimal? AddedValue { get; set; }

        public ICollection<ItemGameAddOns> ItemGameAddOns { get; set; } = [];
        public ICollection<TeamFortressPaintAddOn> TeamFortressPaintAddOns { get; set; } = [];
    }
}
