using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SteamApp.WebAPI.Migrations
{
    /// <inheritdoc />
    public partial class db_v2 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_extra_pixel_game_urls_game_url_id",
                table: "extra_pixel");

            migrationBuilder.DropForeignKey(
                name: "FK_game_urls_game_game_id",
                table: "game_urls");

            migrationBuilder.DropForeignKey(
                name: "FK_product_game_urls_game_url_id",
                table: "product");

            migrationBuilder.DropPrimaryKey(
                name: "PK_game_urls",
                table: "game_urls");

            migrationBuilder.RenameTable(
                name: "game_urls",
                newName: "game_url");

            migrationBuilder.RenameIndex(
                name: "IX_game_urls_game_id",
                table: "game_url",
                newName: "IX_game_url_game_id");

            migrationBuilder.AlterColumn<string>(
                name: "name",
                table: "watch_list",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AlterColumn<string>(
                name: "description",
                table: "watch_list",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AlterColumn<string>(
                name: "batch_url",
                table: "watch_list",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AlterColumn<string>(
                name: "name",
                table: "product",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AlterColumn<string>(
                name: "name",
                table: "game",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AlterColumn<string>(
                name: "base_url",
                table: "game",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AlterColumn<string>(
                name: "partial_url",
                table: "game_url",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AddPrimaryKey(
                name: "PK_game_url",
                table: "game_url",
                column: "id");

            migrationBuilder.CreateIndex(
                name: "IX_watch_list_game_url_id",
                table: "watch_list",
                column: "game_url_id");

            migrationBuilder.AddForeignKey(
                name: "FK_extra_pixel_game_url_game_url_id",
                table: "extra_pixel",
                column: "game_url_id",
                principalTable: "game_url",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_game_url_game_game_id",
                table: "game_url",
                column: "game_id",
                principalTable: "game",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_product_game_url_game_url_id",
                table: "product",
                column: "game_url_id",
                principalTable: "game_url",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_watch_list_game_url_game_url_id",
                table: "watch_list",
                column: "game_url_id",
                principalTable: "game_url",
                principalColumn: "id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_extra_pixel_game_url_game_url_id",
                table: "extra_pixel");

            migrationBuilder.DropForeignKey(
                name: "FK_game_url_game_game_id",
                table: "game_url");

            migrationBuilder.DropForeignKey(
                name: "FK_product_game_url_game_url_id",
                table: "product");

            migrationBuilder.DropForeignKey(
                name: "FK_watch_list_game_url_game_url_id",
                table: "watch_list");

            migrationBuilder.DropIndex(
                name: "IX_watch_list_game_url_id",
                table: "watch_list");

            migrationBuilder.DropPrimaryKey(
                name: "PK_game_url",
                table: "game_url");

            migrationBuilder.RenameTable(
                name: "game_url",
                newName: "game_urls");

            migrationBuilder.RenameIndex(
                name: "IX_game_url_game_id",
                table: "game_urls",
                newName: "IX_game_urls_game_id");

            migrationBuilder.AlterColumn<string>(
                name: "name",
                table: "watch_list",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "description",
                table: "watch_list",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "batch_url",
                table: "watch_list",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "name",
                table: "product",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "name",
                table: "game",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "base_url",
                table: "game",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "partial_url",
                table: "game_urls",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.AddPrimaryKey(
                name: "PK_game_urls",
                table: "game_urls",
                column: "id");

            migrationBuilder.AddForeignKey(
                name: "FK_extra_pixel_game_urls_game_url_id",
                table: "extra_pixel",
                column: "game_url_id",
                principalTable: "game_urls",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_game_urls_game_game_id",
                table: "game_urls",
                column: "game_id",
                principalTable: "game",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_product_game_urls_game_url_id",
                table: "product",
                column: "game_url_id",
                principalTable: "game_urls",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
