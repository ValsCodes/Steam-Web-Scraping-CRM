using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SteamApp.Models.DTOs.GameUrl
{
    public sealed class GameUrlDto
    {
        public long Id { get; set; }
        public long GameId { get; set; }
        public string PartialUrl { get; set; }
        public bool IsBatchUrl { get; set; }
        public int? StartPage { get; set; }
        public int? EndPage { get; set; }
        public bool IsPixelScrape { get; set; }
    }
}
