using System.ComponentModel.DataAnnotations.Schema;

namespace SteamApp.Models.Entities.ManyToMany
{
    [Table("item_slots")]
    public class ItemSlots
    {
        [Column("item_id")]
        public long ItemId { get; set; }
        [ForeignKey(nameof(ItemId))]
        public Item Item { get; set; }

        [Column("slot_id")]
        public long SlotId { get; set; }
        [ForeignKey(nameof(SlotId))]
        public Slot Slot { get; set; }
    }
}
