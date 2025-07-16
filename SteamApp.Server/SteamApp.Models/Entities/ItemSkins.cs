using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SteamApp.Models.Entities;

[Table("item_skins")]
public class ItemSkins
{
    [Key]
    [Column("id")]
    public long Id { get; set; }

    [Column("item_id")]
    public virtual long ItemId { get; set; }
    [ForeignKey(nameof(ItemId))]
    public virtual Item? ManualSearchItem { get; set; }

    [Column("skin_id")]
    public virtual long SkinId { get; set; }
    [ForeignKey(nameof(SkinId))]
    public virtual Skin? Skin { get; set; }

    [Column("grade_id")]
    public virtual long? GradeId { get; set; }

    [ForeignKey(nameof(GradeId))]
    public virtual Grade? Grade { get; set; }
}
