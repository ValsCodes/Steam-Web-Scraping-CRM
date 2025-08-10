using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SteamApp.Models.Entities.OneToOne
{
    [Table("team_fortress_item")]
    public class TeamFortressItem
    {
        [Key]
        [Column("item_id")]
        public long ItemId { get; set; }
        [ForeignKey(nameof(ItemId))]
        public Item Item { get; set; }

        [Column("is_hat")]
        public bool IsHat { get; set; }

        [Column("is_weapon")]
        public bool IsWeapon { get; set; }
    }
}
