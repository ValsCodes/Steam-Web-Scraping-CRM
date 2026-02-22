using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SteamApp.WebAPI.Migrations
{
    /// <inheritdoc />
    public partial class CustomWatchlistRework : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_watch_list_game_url_game_url_id",
                table: "watch_list");

            migrationBuilder.DropForeignKey(
                name: "FK_watch_list_product_product_id",
                table: "watch_list");

            migrationBuilder.DropColumn(
                name: "batch_number",
                table: "watch_list");

            migrationBuilder.RenameColumn(
                name: "product_id",
                table: "watch_list",
                newName: "ProductId");

            migrationBuilder.RenameColumn(
                name: "game_url_id",
                table: "watch_list",
                newName: "GameUrlId");

            migrationBuilder.RenameColumn(
                name: "custom_url",
                table: "watch_list",
                newName: "url");

            migrationBuilder.RenameIndex(
                name: "IX_watch_list_product_id",
                table: "watch_list",
                newName: "IX_watch_list_ProductId");

            migrationBuilder.RenameIndex(
                name: "IX_watch_list_game_url_id",
                table: "watch_list",
                newName: "IX_watch_list_GameUrlId");

            migrationBuilder.AddForeignKey(
                name: "FK_watch_list_game_url_GameUrlId",
                table: "watch_list",
                column: "GameUrlId",
                principalTable: "game_url",
                principalColumn: "id");

            migrationBuilder.AddForeignKey(
                name: "FK_watch_list_product_ProductId",
                table: "watch_list",
                column: "ProductId",
                principalTable: "product",
                principalColumn: "id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_watch_list_game_url_GameUrlId",
                table: "watch_list");

            migrationBuilder.DropForeignKey(
                name: "FK_watch_list_product_ProductId",
                table: "watch_list");

            migrationBuilder.RenameColumn(
                name: "ProductId",
                table: "watch_list",
                newName: "product_id");

            migrationBuilder.RenameColumn(
                name: "GameUrlId",
                table: "watch_list",
                newName: "game_url_id");

            migrationBuilder.RenameColumn(
                name: "url",
                table: "watch_list",
                newName: "custom_url");

            migrationBuilder.RenameIndex(
                name: "IX_watch_list_ProductId",
                table: "watch_list",
                newName: "IX_watch_list_product_id");

            migrationBuilder.RenameIndex(
                name: "IX_watch_list_GameUrlId",
                table: "watch_list",
                newName: "IX_watch_list_game_url_id");

            migrationBuilder.AddColumn<long>(
                name: "batch_number",
                table: "watch_list",
                type: "bigint",
                nullable: true);

            migrationBuilder.AddForeignKey(
                name: "FK_watch_list_game_url_game_url_id",
                table: "watch_list",
                column: "game_url_id",
                principalTable: "game_url",
                principalColumn: "id");

            migrationBuilder.AddForeignKey(
                name: "FK_watch_list_product_product_id",
                table: "watch_list",
                column: "product_id",
                principalTable: "product",
                principalColumn: "id");
        }
    }
}
