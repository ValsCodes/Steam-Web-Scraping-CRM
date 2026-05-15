using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SteamApp.WebAPI.Migrations
{
    /// <inheritdoc />
    public partial class AddScrapingModeToGameUrl : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<long>(
                name: "scraping_mode_id",
                table: "game_url",
                type: "bigint",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "scraping_mode",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    name = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_scraping_mode", x => x.id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_game_url_scraping_mode_id",
                table: "game_url",
                column: "scraping_mode_id");

            migrationBuilder.AddForeignKey(
                name: "FK_game_url_scraping_mode_scraping_mode_id",
                table: "game_url",
                column: "scraping_mode_id",
                principalTable: "scraping_mode",
                principalColumn: "id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_game_url_scraping_mode_scraping_mode_id",
                table: "game_url");

            migrationBuilder.DropTable(
                name: "scraping_mode");

            migrationBuilder.DropIndex(
                name: "IX_game_url_scraping_mode_id",
                table: "game_url");

            migrationBuilder.DropColumn(
                name: "scraping_mode_id",
                table: "game_url");
        }
    }
}
