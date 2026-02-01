using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SteamApp.WebAPI.Migrations
{
    /// <inheritdoc />
    public partial class FixProductsTag : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_product_tags_tag_game_url_id",
                table: "product_tags");

            migrationBuilder.RenameColumn(
                name: "game_url_id",
                table: "product_tags",
                newName: "tag_id");

            migrationBuilder.RenameIndex(
                name: "IX_product_tags_game_url_id",
                table: "product_tags",
                newName: "IX_product_tags_tag_id");

            migrationBuilder.AddForeignKey(
                name: "FK_product_tags_tag_tag_id",
                table: "product_tags",
                column: "tag_id",
                principalTable: "tag",
                principalColumn: "id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_product_tags_tag_tag_id",
                table: "product_tags");

            migrationBuilder.RenameColumn(
                name: "tag_id",
                table: "product_tags",
                newName: "game_url_id");

            migrationBuilder.RenameIndex(
                name: "IX_product_tags_tag_id",
                table: "product_tags",
                newName: "IX_product_tags_game_url_id");

            migrationBuilder.AddForeignKey(
                name: "FK_product_tags_tag_game_url_id",
                table: "product_tags",
                column: "game_url_id",
                principalTable: "tag",
                principalColumn: "id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
