using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SteamApp.Models.Entities
{
    [Table("invoice")]
    public class Invoice
    {
        [Key]
        [Column("id")]
        public string Id { get; set; }

        [Column("game_id")]
        public long GameId { get; set; }
        [ForeignKey(nameof(GameId))]
        public Game Game { get; set; }

        [Column("transcation_date")]
        public DateTime TransactionDate { get; set; }

        [Column("amount")]
        public double Amount { get; set; }
    }
}
