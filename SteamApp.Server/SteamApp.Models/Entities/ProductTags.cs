using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations.Schema;

namespace SteamApp.Domain.Entities;

[PrimaryKey(nameof(TagId), nameof(ProductId))]
[Table("product_tags")]
public class ProductTags
{
    [Column("tag_id")]
    [ForeignKey(nameof(Tag))]
    public long TagId { get; set; }
    [InverseProperty(nameof(Tag.ProductTags))]
    public Tag Tag { get; set; }

    [Column("product_id")]
    [ForeignKey(nameof(Product))]
    public long ProductId { get; set; }
    [InverseProperty(nameof(Product.ProductTags))]
    public Product Product { get; set; }
}
