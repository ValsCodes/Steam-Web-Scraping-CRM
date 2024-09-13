using Microsoft.EntityFrameworkCore.Metadata.Internal;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace SteamAppServer.Models
{
    [Table("sell_listing")]
    public class SellListing
    {
        [Key]
        [Column("id")]
        public long Id { get; private set; }

        [Column("name")]
        public string? Name { get; set; }

        [Column("desciption")]
        public string? Description { get; set; }

        [Column("date_bought")]
        public DateTime DateBought { get; set; }

        [Column("date_sold")]
        public DateTime? DateSold { get; set; }

        [Column("cost_price")]
        public decimal CostPrice { get; set; }

        [Column("t_sell_price_1")]
        public decimal TargetSellPrice1 { get; set; }

        [Column("t_sell_price_2")]
        public decimal? TargetSellPrice2 { get; set; }

        [Column("t_sell_price_3")]
        public decimal? TargetSellPrice3 { get; set; }

        [Column("t_sell_price_4")]
        public decimal? TargetSellPrice4 { get; set; }

        [Column("sold_price")]
        public decimal? SoldPrice { get; set; }

        [Column("is_hat")]
        public bool IsHat { get; set; }

        [Column("is_weapon")]
        public bool IsWeapon { get; set; }

        [Column("is_sold")]
        public bool IsSold { get; set; }
    }
}
