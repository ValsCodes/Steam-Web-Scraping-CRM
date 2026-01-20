namespace SteamApp.Application.DTOs.Product
{
    public sealed class ProductDto
    {
        public long Id { get; set; }
        public long GameUrlId { get; set; }
        public string? Name { get; set; }
        public string? FullUrl { get; set; }
    }
}
