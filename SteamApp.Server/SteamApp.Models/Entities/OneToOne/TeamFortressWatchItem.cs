using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SteamApp.Models.Entities.OneToOne
{
    public class TeamFortressWatchItem
    {
        [Key]
        [Column("watch_item_id")]
        public virtual long WatchItemId { get; set; }
        [ForeignKey(nameof(WatchItemId))]
        public virtual WatchItem WatchItem { get; set; }

        [Column("is_hat")]
        public virtual bool IsHat { get; set; }

        [Column("is_weapon")]
        public virtual bool IsWeapon { get; set; }
    }
}
