﻿namespace SteamApp.Models.DTOs.Product
{
    public class CreateProductDto
    {
        public virtual string Name { get; set; }
        public virtual short? QualityId { get; set; }
        public virtual string? Description { get; set; }
        public virtual DateTime? DateBought { get; set; } = DateTime.UtcNow;
        public virtual DateTime? DateSold { get; set; }
        public virtual decimal? CostPrice { get; set; }
        public virtual decimal? TargetSellPrice1 { get; set; }
        public virtual decimal? TargetSellPrice2 { get; set; }
        public virtual decimal? TargetSellPrice3 { get; set; }
        public virtual decimal? TargetSellPrice4 { get; set; }
        public virtual decimal? SoldPrice { get; set; }
        public virtual bool? IsHat { get; set; } = false;
        public virtual bool? IsWeapon { get; set; } = false;
        public virtual bool? IsSold { get; set; } = false;
        public virtual bool? IsStrange { get; set; }
        public virtual short? PaintId { get; set; }
        public virtual short? SheenId { get; set; }
    }
}
