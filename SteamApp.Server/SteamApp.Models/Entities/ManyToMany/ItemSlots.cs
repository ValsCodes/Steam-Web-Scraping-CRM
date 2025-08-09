using System.ComponentModel.DataAnnotations.Schema;

namespace SteamApp.Models.Entities.ManyToMany
{

    // TODO decalare multiple keys in DataSet
    [Table("item_slots")]
    public class ItemSlots
    {
        [Column("item_id")]
        public long ItemId { get; set; }
        [ForeignKey(nameof(ItemId))]
        public virtual Item Item { get; set; }

        [Column("slot_id")]
        public long SlotId { get; set; }
        [ForeignKey(nameof(SlotId))]
        public virtual Slot Slot { get; set; }
    }
}
