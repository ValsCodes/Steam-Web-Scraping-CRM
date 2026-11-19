using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SteamApp.Migrations
{
    /// <inheritdoc />
    public partial class AddAutomatedScrapeHistory : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "automated_scrape_history",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    user_id = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: true),
                    endpoint = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    scrape_type = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    game_url_id = table.Column<long>(type: "bigint", nullable: false),
                    page = table.Column<short>(type: "smallint", nullable: false),
                    setup_json = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    results_json = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    result_count = table.Column<int>(type: "int", nullable: false),
                    date = table.Column<DateTime>(type: "datetime2", nullable: false),
                    error_text = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    is_have_error = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_automated_scrape_history", x => x.id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_automated_scrape_history_game_url_id",
                table: "automated_scrape_history",
                column: "game_url_id");

            migrationBuilder.CreateIndex(
                name: "IX_automated_scrape_history_user_id_date",
                table: "automated_scrape_history",
                columns: new[] { "user_id", "date" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "automated_scrape_history");
        }
    }
}
