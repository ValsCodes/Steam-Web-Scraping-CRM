using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SteamApp.WebAPI.Migrations
{
    /// <inheritdoc />
    public partial class GameInternalId_GameUrlPixelLocation : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "is_public_api",
                table: "game_url",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<int>(
                name: "pixel_image_height",
                table: "game_url",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "pixel_image_width",
                table: "game_url",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "pixel_x",
                table: "game_url",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "pixel_y",
                table: "game_url",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<long>(
                name: "internal_id",
                table: "game",
                type: "bigint",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "is_public_api",
                table: "game_url");

            migrationBuilder.DropColumn(
                name: "pixel_image_height",
                table: "game_url");

            migrationBuilder.DropColumn(
                name: "pixel_image_width",
                table: "game_url");

            migrationBuilder.DropColumn(
                name: "pixel_x",
                table: "game_url");

            migrationBuilder.DropColumn(
                name: "pixel_y",
                table: "game_url");

            migrationBuilder.DropColumn(
                name: "internal_id",
                table: "game");
        }
    }
}
