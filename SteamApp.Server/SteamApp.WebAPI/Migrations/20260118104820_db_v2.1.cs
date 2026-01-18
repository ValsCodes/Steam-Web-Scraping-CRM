using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SteamApp.WebAPI.Migrations
{
    /// <inheritdoc />
    public partial class db_v21 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "extra_pixel");

            migrationBuilder.CreateTable(
                name: "pixel",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    game_url_id = table.Column<long>(type: "bigint", nullable: false),
                    value = table.Column<long>(type: "bigint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_pixel", x => x.id);
                    table.ForeignKey(
                        name: "FK_pixel_game_url_game_url_id",
                        column: x => x.game_url_id,
                        principalTable: "game_url",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_pixel_game_url_id",
                table: "pixel",
                column: "game_url_id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "pixel");

            migrationBuilder.CreateTable(
                name: "extra_pixel",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    game_url_id = table.Column<long>(type: "bigint", nullable: false),
                    pixel_value = table.Column<long>(type: "bigint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_extra_pixel", x => x.id);
                    table.ForeignKey(
                        name: "FK_extra_pixel_game_url_game_url_id",
                        column: x => x.game_url_id,
                        principalTable: "game_url",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_extra_pixel_game_url_id",
                table: "extra_pixel",
                column: "game_url_id");
        }
    }
}
