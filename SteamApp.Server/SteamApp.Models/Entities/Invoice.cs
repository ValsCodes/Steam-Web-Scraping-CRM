using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SteamApp.Models.Entities
{
    [Table("invoice")]
    public class Invoice
    {
        [Key]
        [Column("id")]
        public virtual string Id { get; set; }

        [Column("transcation_date")]
        public virtual DateTime TransactionDate { get; set; }

        [Column("amount")]
        public virtual double Amount { get; set; }
    }
}
