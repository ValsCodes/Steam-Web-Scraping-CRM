namespace SteamApp.Models.DTOs.Product
{
    public class UpdateProductDto
    {
        public long Id { get; set; }
        public string? Name { get; set; }
        public int? QualityId { get; set; }
        public string? Description { get; set; }
        public DateTime? DateBought { get; set; }
        public DateTime? DateSold { get; set; }
        public decimal? CostPrice { get; set; }
        public decimal? TargetSellPrice1 { get; set; }
        public decimal? TargetSellPrice2 { get; set; }
        public decimal? TargetSellPrice3 { get; set; }
        public decimal? TargetSellPrice4 { get; set; }
        public decimal? SoldPrice { get; set; }
        public bool? IsHat { get; set; }
        public bool? IsWeapon { get; set; }
        public bool? IsSold { get; set; }
        public virtual bool? IsStrange { get; set; }
        public virtual short? PaintId { get; set; }
        public virtual short? SheenId { get; set; }
    }
}
