using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SteamApp.WebAPI.Migrations
{
    /// <inheritdoc />
    public partial class ProductTags : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "tag",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    game_id = table.Column<long>(type: "bigint", nullable: false),
                    name = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_tag", x => x.id);
                    table.ForeignKey(
                        name: "FK_tag_game_game_id",
                        column: x => x.game_id,
                        principalTable: "game",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "product_tags",
                columns: table => new
                {
                    game_url_id = table.Column<long>(type: "bigint", nullable: false),
                    product_id = table.Column<long>(type: "bigint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_product_tags", x => new { x.product_id, x.game_url_id });
                    table.ForeignKey(
                        name: "FK_product_tags_product_product_id",
                        column: x => x.product_id,
                        principalTable: "product",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_product_tags_tag_game_url_id",
                        column: x => x.game_url_id,
                        principalTable: "tag",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_product_tags_game_url_id",
                table: "product_tags",
                column: "game_url_id");

            migrationBuilder.CreateIndex(
                name: "IX_tag_game_id",
                table: "tag",
                column: "game_id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "product_tags");

            migrationBuilder.DropTable(
                name: "tag");
        }
    }
}
