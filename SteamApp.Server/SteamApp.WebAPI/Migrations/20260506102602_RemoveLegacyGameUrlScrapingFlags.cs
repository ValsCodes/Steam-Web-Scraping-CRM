using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SteamApp.WebAPI.Migrations
{
    /// <inheritdoc />
    public partial class RemoveLegacyGameUrlScrapingFlags : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "is_batch_url",
                table: "game_url");

            migrationBuilder.DropColumn(
                name: "is_pixel_scrape",
                table: "game_url");

            migrationBuilder.DropColumn(
                name: "is_public_api",
                table: "game_url");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "is_batch_url",
                table: "game_url",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "is_pixel_scrape",
                table: "game_url",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "is_public_api",
                table: "game_url",
                type: "bit",
                nullable: false,
                defaultValue: false);
        }
    }
}
