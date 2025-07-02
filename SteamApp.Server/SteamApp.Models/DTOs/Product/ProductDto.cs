namespace SteamApp.Models.DTOs.Product
{
    public class ProductDto
    {
        public long Id { get; set; }
        public virtual string Name { get; set; }
        public virtual short? QualityId { get; set; }

        public virtual string? Description { get; set; }

        public virtual DateTime DateBought { get; set; }

        public virtual DateTime? DateSold { get; set; }

        public virtual decimal? CostPrice { get; set; }

        public virtual decimal? TargetSellPrice1 { get; set; }

        public virtual decimal? TargetSellPrice2 { get; set; }

        public virtual decimal? TargetSellPrice3 { get; set; }

        public virtual decimal? TargetSellPrice4 { get; set; }

        public virtual decimal? SoldPrice { get; set; }

        public virtual bool IsHat { get; set; }

        public virtual bool IsWeapon { get; set; }

        public virtual bool? IsSold { get; set; }
    }
}
