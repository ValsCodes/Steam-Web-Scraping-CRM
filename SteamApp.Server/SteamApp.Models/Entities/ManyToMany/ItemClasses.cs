using System.ComponentModel.DataAnnotations.Schema;

namespace SteamApp.Models.Entities.ManyToMany
{
    [Table("item_classes")]
    public class ItemClasses
    {
        [Column("item_id")]
        public long ItemId { get; set; }
        [ForeignKey(nameof(ItemId))]
        public Item Item { get; set; }

        [Column("class_id")]
        public long ClassId { get; set; }
        [ForeignKey(nameof(ClassId))]
        public Class Class { get; set; }
    }
}
