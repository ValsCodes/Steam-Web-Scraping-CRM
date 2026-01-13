using System.ComponentModel.DataAnnotations.Schema;

namespace SteamApp.Models.Entities
{
    [Table("watch_list")]
    public sealed class WatchListItem
    {
        [Column("id")]
        public long Id { get; set; }

        [Column("game_id")]
        public long? GameId { get; set; }

        [Column("game_url_id")]
        public long? GameUrlId { get; set; }

        [Column("rating")]
        public int? Rating { get; set; }

        [Column("batch_url")]
        public string BatchUrl { get; set; }

        [Column("name")]
        public string Name { get; set; }

        [Column("rd")]
        public DateOnly ReleaseDate { get; set; }

        [Column("description")]
        public string Description { get; set; }
    }

}
