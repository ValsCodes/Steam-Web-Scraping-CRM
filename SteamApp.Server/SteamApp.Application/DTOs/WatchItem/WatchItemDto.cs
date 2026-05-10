namespace SteamApp.Application.DTOs.WatchItem
{
    public class WatchItemDto
    {
        public string? PageUrl { get; set; }
        public string? Name { get; set; }
        public double Price { get; set; }
        public string PixelName { get; set; } = string.Empty;
        public string? ImageUrl { get; set; }
        public string ListingUrl { get; set; } = string.Empty;
        public short Quantity { get; set; }
        public bool IsPainted { get; set; }
        public string? PaintText { get; set; }
        public bool IsGoodPaint { get; set; }
        public long RedValue { get; set; }
        public long GreenValue { get; set; }
        public long BlueValue { get; set; }
    }
}
