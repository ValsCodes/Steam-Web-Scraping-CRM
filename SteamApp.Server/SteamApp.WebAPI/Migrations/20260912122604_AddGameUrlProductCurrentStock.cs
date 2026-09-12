using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SteamApp.WebAPI.Migrations
{
    /// <inheritdoc />
    public partial class AddGameUrlProductCurrentStock : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "current_stock",
                table: "game_url_products",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateTable(
                name: "game_url_product_stock_history",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    product_id = table.Column<long>(type: "bigint", nullable: false),
                    game_url_id = table.Column<long>(type: "bigint", nullable: false),
                    previous_stock = table.Column<int>(type: "int", nullable: false),
                    new_stock = table.Column<int>(type: "int", nullable: false),
                    operation = table.Column<int>(type: "int", nullable: false),
                    created_at_utc = table.Column<DateTime>(type: "datetime2", nullable: false),
                    user_id = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_game_url_product_stock_history", x => x.id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_game_url_product_stock_history_user_id_product_id_game_url_id_created_at_utc",
                table: "game_url_product_stock_history",
                columns: new[] { "user_id", "product_id", "game_url_id", "created_at_utc" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "game_url_product_stock_history");

            migrationBuilder.DropColumn(
                name: "current_stock",
                table: "game_url_products");
        }
    }
}
