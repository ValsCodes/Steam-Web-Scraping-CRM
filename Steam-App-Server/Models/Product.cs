using Microsoft.EntityFrameworkCore.Metadata.Internal;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using Microsoft.EntityFrameworkCore;

namespace SteamAppServer.Models
{
    [Table("sell_listing")]
    public class Product
    {
        [Key]
        [Column("id")]
        public virtual  long Id { get; set; }

        [Column("name")]
        public virtual string? Name { get; set; }

        [Column("quality_id")]
        public virtual short? QualityId { get; set; }

        [Column("desciption")]
        public virtual string? Description { get; set; }

        [Column("date_bought")]
        public virtual DateTime DateBought { get; set; }

        [Column("date_sold")]
        public virtual DateTime? DateSold { get; set; }

        [Column("cost_price")]
        [Precision(18, 2)]
        public decimal CostPrice { get; set; }

        [Column("t_sell_price_1")]
        [Precision(18, 2)]
        public virtual decimal TargetSellPrice1 { get; set; }
        

        [Column("t_sell_price_2")]
        [Precision(18, 2)]
        public virtual decimal? TargetSellPrice2 { get; set; }

        [Column("t_sell_price_3")]
        [Precision(18, 2)]
        public virtual decimal? TargetSellPrice3 { get; set; }

        [Column("t_sell_price_4")]
        [Precision(18, 2)]
        public virtual decimal? TargetSellPrice4 { get; set; }

        [Column("sold_price")]
        [Precision(18, 2)]
        public virtual decimal? SoldPrice { get; set; }

        [Column("is_hat")]
        public virtual bool IsHat { get; set; }

        [Column("is_weapon")]
        public virtual bool IsWeapon { get; set; }

        [Column("is_sold")]
        public virtual bool IsSold { get; set; }
    }
}
