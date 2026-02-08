using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SteamApp.WebAPI.Migrations
{
    /// <inheritdoc />
    public partial class CustomWatchlistUrl : Migration
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

            migrationBuilder.RenameColumn(
                name: "description",
                table: "watch_list",
                newName: "custom_url");

            migrationBuilder.AlterColumn<long>(
                name: "product_id",
                table: "watch_list",
                type: "bigint",
                nullable: true,
                oldClrType: typeof(long),
                oldType: "bigint");

            migrationBuilder.AlterColumn<long>(
                name: "game_url_id",
                table: "watch_list",
                type: "bigint",
                nullable: true,
                oldClrType: typeof(long),
                oldType: "bigint");

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

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_watch_list_game_url_game_url_id",
                table: "watch_list");

            migrationBuilder.DropForeignKey(
                name: "FK_watch_list_product_product_id",
                table: "watch_list");

            migrationBuilder.RenameColumn(
                name: "custom_url",
                table: "watch_list",
                newName: "description");

            migrationBuilder.AlterColumn<long>(
                name: "product_id",
                table: "watch_list",
                type: "bigint",
                nullable: false,
                defaultValue: 0L,
                oldClrType: typeof(long),
                oldType: "bigint",
                oldNullable: true);

            migrationBuilder.AlterColumn<long>(
                name: "game_url_id",
                table: "watch_list",
                type: "bigint",
                nullable: false,
                defaultValue: 0L,
                oldClrType: typeof(long),
                oldType: "bigint",
                oldNullable: true);

            migrationBuilder.AddForeignKey(
                name: "FK_watch_list_game_url_game_url_id",
                table: "watch_list",
                column: "game_url_id",
                principalTable: "game_url",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_watch_list_product_product_id",
                table: "watch_list",
                column: "product_id",
                principalTable: "product",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
