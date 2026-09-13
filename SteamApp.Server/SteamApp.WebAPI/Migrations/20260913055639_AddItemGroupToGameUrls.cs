using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SteamApp.WebAPI.Migrations
{
    /// <inheritdoc />
    public partial class AddItemGroupToGameUrls : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<long>(
                name: "item_group_id",
                table: "game_url",
                type: "bigint",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_game_url_item_group_id",
                table: "game_url",
                column: "item_group_id");

            migrationBuilder.AddForeignKey(
                name: "FK_game_url_item_group_item_group_id",
                table: "game_url",
                column: "item_group_id",
                principalTable: "item_group",
                principalColumn: "id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_game_url_item_group_item_group_id",
                table: "game_url");

            migrationBuilder.DropIndex(
                name: "IX_game_url_item_group_id",
                table: "game_url");

            migrationBuilder.DropColumn(
                name: "item_group_id",
                table: "game_url");
        }
    }
}
