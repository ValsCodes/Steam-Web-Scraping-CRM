namespace SteamApp.Models.DTOs
{
    public class ListingDto
    {
        public virtual string? Name { get; set; }
        public virtual double Price { get; set; }
        public virtual string Color { get; set; }
        public virtual string? ImageUrl { get; set; }
        public virtual string ListingUrl { get; set; }
        public virtual short Quantity { get; set; }
        public virtual bool IsPainted { get; set; }
        public virtual string? PaintText { get; set; }
        public virtual bool IsGoodPaint { get; set; }
    }
}
