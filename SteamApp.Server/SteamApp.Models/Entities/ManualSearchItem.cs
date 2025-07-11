using System.ComponentModel.DataAnnotations.Schema;

namespace SteamApp.Models.Entities
{
    [Table("item")]
    public class ManualSearchItem : BaseModel
    {
        [Column("is_active")]
        public bool IsActive { get; set; }

        [Column("is_weapon")]
        public bool IsWeapon { get; set; }

        [Column("class_id")]
        public long? ClassId { get; set; }

        [Column("slot_id")]
        public long? SlotId { get; set; }

        [Column("current_stock")]
        public int? CurrentStock { get; set; }
    }
}
