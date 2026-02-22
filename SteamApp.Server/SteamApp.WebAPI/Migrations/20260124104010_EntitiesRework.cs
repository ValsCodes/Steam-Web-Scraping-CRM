using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SteamApp.WebAPI.Migrations
{
    /// <inheritdoc />
    public partial class EntitiesRework : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_pixel_game_url_game_url_id",
                table: "pixel");

            migrationBuilder.DropForeignKey(
                name: "FK_product_game_url_game_url_id",
                table: "product");

            migrationBuilder.DropForeignKey(
                name: "FK_watch_list_game_game_id",
                table: "watch_list");

            migrationBuilder.DropForeignKey(
                name: "FK_watch_list_game_url_game_url_id",
                table: "watch_list");

            migrationBuilder.DropIndex(
                name: "IX_watch_list_game_id",
                table: "watch_list");

            migrationBuilder.DropColumn(
                name: "batch_url",
                table: "watch_list");

            migrationBuilder.RenameColumn(
                name: "game_id",
                table: "watch_list",
                newName: "batch_number");

            migrationBuilder.RenameColumn(
                name: "game_url_id",
                table: "product",
                newName: "game_id");

            migrationBuilder.RenameIndex(
                name: "IX_product_game_url_id",
                table: "product",
                newName: "IX_product_game_id");

            migrationBuilder.RenameColumn(
                name: "value",
                table: "pixel",
                newName: "r_value");

            migrationBuilder.RenameColumn(
                name: "game_url_id",
                table: "pixel",
                newName: "game_id");

            migrationBuilder.RenameIndex(
                name: "IX_pixel_game_url_id",
                table: "pixel",
                newName: "IX_pixel_game_id");

            migrationBuilder.AddColumn<string>(
                name: "name",
                table: "wish_list",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AlterColumn<long>(
                name: "game_url_id",
                table: "watch_list",
                type: "bigint",
                nullable: false,
                defaultValue: 0L,
                oldClrType: typeof(long),
                oldType: "bigint",
                oldNullable: true);

            migrationBuilder.AddColumn<long>(
                name: "product_id",
                table: "watch_list",
                type: "bigint",
                nullable: false,
                defaultValue: 0L);

            migrationBuilder.AddColumn<long>(
                name: "b_value",
                table: "pixel",
                type: "bigint",
                nullable: false,
                defaultValue: 0L);

            migrationBuilder.AddColumn<long>(
                name: "g_value",
                table: "pixel",
                type: "bigint",
                nullable: false,
                defaultValue: 0L);

            migrationBuilder.AddColumn<string>(
                name: "name",
                table: "pixel",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "page_url",
                table: "game",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "game_add_on",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    name = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    game_id = table.Column<long>(type: "bigint", nullable: false),
                    price = table.Column<double>(type: "float", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_game_add_on", x => x.id);
                    table.ForeignKey(
                        name: "FK_game_add_on_game_game_id",
                        column: x => x.game_id,
                        principalTable: "game",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "game_url_pixels",
                columns: table => new
                {
                    game_url_id = table.Column<long>(type: "bigint", nullable: false),
                    pixel_id = table.Column<long>(type: "bigint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_game_url_pixels", x => new { x.pixel_id, x.game_url_id });
                    table.ForeignKey(
                        name: "FK_game_url_pixels_game_url_game_url_id",
                        column: x => x.game_url_id,
                        principalTable: "game_url",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_game_url_pixels_pixel_pixel_id",
                        column: x => x.pixel_id,
                        principalTable: "pixel",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "game_url_products",
                columns: table => new
                {
                    game_url_id = table.Column<long>(type: "bigint", nullable: false),
                    product_id = table.Column<long>(type: "bigint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_game_url_products", x => new { x.product_id, x.game_url_id });
                    table.ForeignKey(
                        name: "FK_game_url_products_game_url_game_url_id",
                        column: x => x.game_url_id,
                        principalTable: "game_url",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_game_url_products_product_product_id",
                        column: x => x.product_id,
                        principalTable: "product",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_watch_list_product_id",
                table: "watch_list",
                column: "product_id");

            migrationBuilder.CreateIndex(
                name: "IX_game_add_on_game_id",
                table: "game_add_on",
                column: "game_id");

            migrationBuilder.CreateIndex(
                name: "IX_game_url_pixels_game_url_id",
                table: "game_url_pixels",
                column: "game_url_id");

            migrationBuilder.CreateIndex(
                name: "IX_game_url_products_game_url_id",
                table: "game_url_products",
                column: "game_url_id");

            migrationBuilder.AddForeignKey(
                name: "FK_pixel_game_game_id",
                table: "pixel",
                column: "game_id",
                principalTable: "game",
                principalColumn: "id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_product_game_game_id",
                table: "product",
                column: "game_id",
                principalTable: "game",
                principalColumn: "id",
                onDelete: ReferentialAction.Restrict);

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

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_pixel_game_game_id",
                table: "pixel");

            migrationBuilder.DropForeignKey(
                name: "FK_product_game_game_id",
                table: "product");

            migrationBuilder.DropForeignKey(
                name: "FK_watch_list_game_url_game_url_id",
                table: "watch_list");

            migrationBuilder.DropForeignKey(
                name: "FK_watch_list_product_product_id",
                table: "watch_list");

            migrationBuilder.DropTable(
                name: "game_add_on");

            migrationBuilder.DropTable(
                name: "game_url_pixels");

            migrationBuilder.DropTable(
                name: "game_url_products");

            migrationBuilder.DropIndex(
                name: "IX_watch_list_product_id",
                table: "watch_list");

            migrationBuilder.DropColumn(
                name: "name",
                table: "wish_list");

            migrationBuilder.DropColumn(
                name: "product_id",
                table: "watch_list");

            migrationBuilder.DropColumn(
                name: "b_value",
                table: "pixel");

            migrationBuilder.DropColumn(
                name: "g_value",
                table: "pixel");

            migrationBuilder.DropColumn(
                name: "name",
                table: "pixel");

            migrationBuilder.DropColumn(
                name: "page_url",
                table: "game");

            migrationBuilder.RenameColumn(
                name: "batch_number",
                table: "watch_list",
                newName: "game_id");

            migrationBuilder.RenameColumn(
                name: "game_id",
                table: "product",
                newName: "game_url_id");

            migrationBuilder.RenameIndex(
                name: "IX_product_game_id",
                table: "product",
                newName: "IX_product_game_url_id");

            migrationBuilder.RenameColumn(
                name: "r_value",
                table: "pixel",
                newName: "value");

            migrationBuilder.RenameColumn(
                name: "game_id",
                table: "pixel",
                newName: "game_url_id");

            migrationBuilder.RenameIndex(
                name: "IX_pixel_game_id",
                table: "pixel",
                newName: "IX_pixel_game_url_id");

            migrationBuilder.AlterColumn<long>(
                name: "game_url_id",
                table: "watch_list",
                type: "bigint",
                nullable: true,
                oldClrType: typeof(long),
                oldType: "bigint");

            migrationBuilder.AddColumn<string>(
                name: "batch_url",
                table: "watch_list",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_watch_list_game_id",
                table: "watch_list",
                column: "game_id");

            migrationBuilder.AddForeignKey(
                name: "FK_pixel_game_url_game_url_id",
                table: "pixel",
                column: "game_url_id",
                principalTable: "game_url",
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
                name: "FK_watch_list_game_game_id",
                table: "watch_list",
                column: "game_id",
                principalTable: "game",
                principalColumn: "id");

            migrationBuilder.AddForeignKey(
                name: "FK_watch_list_game_url_game_url_id",
                table: "watch_list",
                column: "game_url_id",
                principalTable: "game_url",
                principalColumn: "id");
        }
    }
}
