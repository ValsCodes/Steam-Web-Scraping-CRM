using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SteamApp.WebAPI.Migrations
{
    /// <inheritdoc />
    public partial class db12 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "game",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    name = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    base_url = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_game", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "game_urls",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    game_id = table.Column<long>(type: "bigint", nullable: false),
                    partial_url = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    is_batch_url = table.Column<bool>(type: "bit", nullable: false),
                    start_page = table.Column<int>(type: "int", nullable: true),
                    end_page = table.Column<int>(type: "int", nullable: true),
                    is_pixel_scrape = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_game_urls", x => x.id);
                    table.ForeignKey(
                        name: "FK_game_urls_game_game_id",
                        column: x => x.game_id,
                        principalTable: "game",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "watch_list",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    game_id = table.Column<long>(type: "bigint", nullable: true),
                    game_url_id = table.Column<long>(type: "bigint", nullable: true),
                    rating = table.Column<int>(type: "int", nullable: true),
                    batch_url = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    name = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    rd = table.Column<DateOnly>(type: "date", nullable: false),
                    description = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_watch_list", x => x.id);
                    table.ForeignKey(
                        name: "FK_watch_list_game_game_id",
                        column: x => x.game_id,
                        principalTable: "game",
                        principalColumn: "id");
                });

            migrationBuilder.CreateTable(
                name: "wish_list",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    game_id = table.Column<long>(type: "bigint", nullable: false),
                    price = table.Column<double>(type: "float", nullable: true),
                    is_active = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_wish_list", x => x.id);
                    table.ForeignKey(
                        name: "FK_wish_list_game_game_id",
                        column: x => x.game_id,
                        principalTable: "game",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

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
                        name: "FK_extra_pixel_game_urls_game_url_id",
                        column: x => x.game_url_id,
                        principalTable: "game_urls",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "product",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    game_url_id = table.Column<long>(type: "bigint", nullable: false),
                    name = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_product", x => x.id);
                    table.ForeignKey(
                        name: "FK_product_game_urls_game_url_id",
                        column: x => x.game_url_id,
                        principalTable: "game_urls",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_extra_pixel_game_url_id",
                table: "extra_pixel",
                column: "game_url_id");

            migrationBuilder.CreateIndex(
                name: "IX_game_urls_game_id",
                table: "game_urls",
                column: "game_id");

            migrationBuilder.CreateIndex(
                name: "IX_product_game_url_id",
                table: "product",
                column: "game_url_id");

            migrationBuilder.CreateIndex(
                name: "IX_watch_list_game_id",
                table: "watch_list",
                column: "game_id");

            migrationBuilder.CreateIndex(
                name: "IX_wish_list_game_id",
                table: "wish_list",
                column: "game_id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "extra_pixel");

            migrationBuilder.DropTable(
                name: "product");

            migrationBuilder.DropTable(
                name: "watch_list");

            migrationBuilder.DropTable(
                name: "wish_list");

            migrationBuilder.DropTable(
                name: "game_urls");

            migrationBuilder.DropTable(
                name: "game");
        }
    }
}
