using System.ComponentModel.DataAnnotations.Schema;

namespace SteamApp.Models.Entities;

[Table("product")]
public class WatchItem : BaseModel
{
    [Column("game_id")]
    public virtual long GameId { get; set; }
    [ForeignKey(nameof(GameId))]
    public virtual Game Game { get; set; }

    [Column("game_url_id")]
    public virtual long GameUrlId { get; set; }
    [ForeignKey(nameof(GameUrlId))]
    public virtual GameUrl GameUrl { get; set; }

    [Column("item_id")]
    public virtual long? ItemId { get; set; }
    [ForeignKey(nameof(ItemId))]
    public virtual Item Item { get; set; }

    // To be removed
    [Column("quality_id")]
    public virtual short? QualityId { get; set; }
    [ForeignKey(nameof(QualityId))]
    public virtual Quality? Quality { get; set; }

    // To be removed
    [Column("paint_id")]
    public virtual short? PaintId { get; set; }

    [ForeignKey(nameof(PaintId))]
    public virtual Paint? Paint { get; set; }

    // To be removed
    [Column("sheen_id")]
    public virtual short? SheenId { get; set; }
    [ForeignKey(nameof(SheenId))]
    public virtual Sheen? Sheen { get; set; }

    [Column("desciption")]
    public virtual string? Description { get; set; }

    // To be removed
    [Column("date_bought")]
    public virtual DateTime DateBought { get; set; }

    // To be removed
    [Column("date_sold")]
    public virtual DateTime? DateSold { get; set; }

    [Column("cost_price")]
    public decimal CostPrice { get; set; }

    [Column("t_sell_price_1")]
    public virtual decimal? TargetSellPrice1 { get; set; }     

    [Column("t_sell_price_2")]
    public virtual decimal? TargetSellPrice2 { get; set; }

    [Column("t_sell_price_3")]
    public virtual decimal? TargetSellPrice3 { get; set; }

    [Column("t_sell_price_4")]
    public virtual decimal? TargetSellPrice4 { get; set; }

    // To be removed
    [Column("sold_price")]
    public virtual decimal? SoldPrice { get; set; }

    // To be removed
    [Column("is_hat")]
    public virtual bool IsHat { get; set; }

    // To be removed
    [Column("is_weapon")]
    public virtual bool IsWeapon { get; set; }

    // To be removed
    [Column("is_sold")]
    public virtual bool IsSold { get; set; }

    // To be removed
    [Column("is_strange")]
    public virtual bool? IsStrange { get; set; }
}
