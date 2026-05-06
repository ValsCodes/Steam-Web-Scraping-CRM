using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using SteamApp.Infrastructure.Context;

#nullable disable

namespace SteamApp.WebAPI.Migrations
{
    [DbContext(typeof(ApplicationDbContext))]
    [Migration("20260506100000_BackfillGameUrlScrapingModes")]
    public partial class BackfillGameUrlScrapingModes : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(
                """
                UPDATE game_url
                SET scraping_mode_id = CASE
                    WHEN is_public_api = 1 THEN 4
                    WHEN is_batch_url = 1 AND is_pixel_scrape = 1 THEN 3
                    WHEN is_batch_url = 1 THEN 2
                    ELSE 1
                END;
                """);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(
                """
                UPDATE game_url
                SET scraping_mode_id = NULL;
                """);
        }
    }
}
