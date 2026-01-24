namespace SteamApp.Application.DTOs.Product
{
    public sealed class ProductDto
    {
        public long Id { get; set; }

        public long GameId { get; set; }
        public string? Name { get; set; }
    }
}
