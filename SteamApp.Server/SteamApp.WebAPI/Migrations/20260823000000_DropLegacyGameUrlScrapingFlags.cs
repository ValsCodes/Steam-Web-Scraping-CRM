using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using SteamApp.Infrastructure.Context;

#nullable disable

namespace SteamApp.WebAPI.Migrations;

[DbContext(typeof(ApplicationDbContext))]
[Migration("20260823000000_DropLegacyGameUrlScrapingFlags")]
public partial class DropLegacyGameUrlScrapingFlags : Migration
{
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
