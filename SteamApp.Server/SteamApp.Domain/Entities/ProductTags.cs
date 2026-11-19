namespace SteamApp.Domain.Entities;
public class ProductTags
{
    public long TagId { get; set; }
    public Tag Tag { get; set; } = null!;

    public long ProductId { get; set; }
    public Product Product { get; set; } = null!;
}
