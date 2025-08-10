using System.ComponentModel.DataAnnotations.Schema;

namespace SteamApp.Models.Entities.ManyToMany
{
    [Table("item_qualities")]
    public class ItemQualities
    {
        [Column("item_id")]
        public long ItemId { get; set; }
        [ForeignKey(nameof(ItemId))]
        public Item Item { get; set; }

        [Column("quality_id")]
        public short QualityId { get; set; }
        [ForeignKey(nameof(QualityId))]
        public Quality Quality { get; set; }
    }
}
