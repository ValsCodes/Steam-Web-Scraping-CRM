using System.ComponentModel.DataAnnotations.Schema;

namespace SteamApp.Models.Entities
{
    [Table("watch_list")]
    public sealed class WatchList
    {
        [Column("id")]
        public long Id { get; set; }

        [Column("game_id")]
        [ForeignKey(nameof(Game))]
        public long? GameId { get; set; }
        [InverseProperty(nameof(Game.WatchLists))]
        public Game Game { get; set; }

        [Column("game_url_id")]
        [ForeignKey(nameof(GameUrl))]
        public long? GameUrlId { get; set; }
        [InverseProperty(nameof(GameUrl.WatchLists))]
        public GameUrl GameUrl { get; set; }

        [Column("rating")]
        public int? Rating { get; set; }

        [Column("batch_url")]
        public string? BatchUrl { get; set; }

        [Column("name")]
        public string? Name { get; set; }

        [Column("rd")]
        public DateOnly ReleaseDate { get; set; }

        [Column("description")]
        public string? Description { get; set; }
    }

}
