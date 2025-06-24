using System.ComponentModel.DataAnnotations.Schema;

namespace SteamApp.Models.Models
{
    [Table("item")]
    public class Item : BaseModel
    {
        [Column("is_active")]
        public bool IsActive { get; set; }

        [Column("is_weapon")]
        public bool IsWeapon { get; set; }

        [Column("class_id")]
        public long? ClassId { get; set; }

        [Column("slot_id")]
        public long? SlotId { get; set; }
    }
}
