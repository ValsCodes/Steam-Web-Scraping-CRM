using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SteamApp.Models.Entities
{
    [Table("target")]
    public class Target
    {
        [Key]
        [Column("id")]
        public long Id { get; set; }

        [Column("watch_item_id")]
        public long WatchItemId { get; set; }
        [ForeignKey(nameof(WatchItemId))]
        public WatchItem WatchItem { get; set; }

        [Column("target_price")]
        public decimal TargetPrice { get; set; }
    }
}
