using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SteamApp.Models.Entities.OneToOne
{
    public class TeamFotressItem
    {
        [Key]
        [Column("item_id")]
        public long ItemId { get; set; }
        [ForeignKey(nameof(ItemId))]
        public virtual Item Item { get; set; }

        [Column("class_id")]
        public long? ClassId { get; set; }
        [ForeignKey(nameof(ClassId))]
        public virtual Class? Class { get; set; }
    }
}
