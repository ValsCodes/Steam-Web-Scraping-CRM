using System.ComponentModel;

namespace SteamApp.Models.Dto
{
    public class ProductDto
    {
        [DefaultValue(0)]
        public virtual long Id { get; set; }

        [DefaultValue("Test Product")]
        public virtual string Name { get; set; }

        [DefaultValue(null)]
        public virtual short? QualityId { get; set; } = null;

        [DefaultValue("")]
        public virtual string Description { get; set; }

        public virtual DateTime DateBought { get; set; }

        [DefaultValue(null)]
        public virtual DateTime? DateSold { get; set; }

        [DefaultValue(0)]
        public virtual decimal CostPrice { get; set; } = decimal.Zero;

        public virtual decimal TargetSellPrice1 { get; set; }

        [DefaultValue(null)]
        public virtual decimal? TargetSellPrice2 { get; set; }

        [DefaultValue(null)]
        public virtual decimal? TargetSellPrice3 { get; set; }

        [DefaultValue(null)]
        public virtual decimal? TargetSellPrice4 { get; set; }

        [DefaultValue(null)]
        public virtual decimal? SoldPrice { get; set; }

        [DefaultValue(true)]
        public virtual bool IsHat { get; set; } = true;

        [DefaultValue(false)]
        public virtual bool IsWeapon { get; set; } = false;

        [DefaultValue(false)]
        public virtual bool IsSold { get; set; } = false;
    }
}
