using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SteamApp.Models.Entities.ManyToMany
{
    // TODO decalare multiple keys in DataSet
    [Table("item_game_add_ons")]
    public  class ItemGameAddOns
    {
        [Column("item_id")]
        public virtual long ItemId { get; set; }
        [ForeignKey(nameof(ItemId))]
        public virtual Item Item { get; set; }

        [Column("game_add_on_id")]
        public virtual long GameAddOnId { get; set; }
        [ForeignKey(nameof(GameAddOnId))]
        public virtual GameAddOn GameAddOn { get; set; }
    }
}
