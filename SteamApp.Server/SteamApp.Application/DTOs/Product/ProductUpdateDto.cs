namespace SteamApp.Application.DTOs.Product
{
    public sealed class ProductUpdateDto
    {
        public long Id { get; set; }
        public string? Name { get; set; }
        public bool? IsActive { get; set; }

    }
}
