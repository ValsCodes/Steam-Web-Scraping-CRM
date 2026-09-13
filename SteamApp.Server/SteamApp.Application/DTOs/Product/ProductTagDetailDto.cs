namespace SteamApp.Application.DTOs.Product;

public sealed class ProductTagDetailDto
{
    public long Id { get; set; }
    public string? Name { get; set; }
    public bool IsActive { get; set; }
    public long? ItemGroupId { get; set; }
    public string? ItemGroupName { get; set; }
}
