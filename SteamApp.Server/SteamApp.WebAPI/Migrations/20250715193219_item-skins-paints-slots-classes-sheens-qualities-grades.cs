using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SteamApp.Migrations
{
    /// <inheritdoc />
    public partial class itemskinspaintsslotsclassessheensqualitiesgrades : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "is_strange",
                table: "product",
                type: "bit",
                nullable: true);

            migrationBuilder.AddColumn<short>(
                name: "paint_id",
                table: "product",
                type: "smallint",
                nullable: true);

            migrationBuilder.AddColumn<short>(
                name: "sheen_id",
                table: "product",
                type: "smallint",
                nullable: true);

            migrationBuilder.AlterColumn<long>(
                name: "slot_id",
                table: "item",
                type: "bigint",
                nullable: true,
                oldClrType: typeof(long),
                oldType: "bigint");

            migrationBuilder.AlterColumn<long>(
                name: "class_id",
                table: "item",
                type: "bigint",
                nullable: true,
                oldClrType: typeof(long),
                oldType: "bigint");

            migrationBuilder.AddColumn<int>(
                name: "current_stock",
                table: "item",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<short>(
                name: "rating",
                table: "item",
                type: "smallint",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "trades_count",
                table: "item",
                type: "int",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "class",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    name = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_class", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "grade",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    name = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_grade", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "paint",
                columns: table => new
                {
                    id = table.Column<short>(type: "smallint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    name = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    r_value = table.Column<byte>(type: "tinyint", nullable: false),
                    g_value = table.Column<byte>(type: "tinyint", nullable: false),
                    b_value = table.Column<byte>(type: "tinyint", nullable: false),
                    is_good_paint = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_paint", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "quality",
                columns: table => new
                {
                    id = table.Column<short>(type: "smallint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    name = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_quality", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "sheen",
                columns: table => new
                {
                    id = table.Column<short>(type: "smallint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    name = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    is_good_sheen = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_sheen", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "slot",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    name = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_slot", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "skin",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    is_war_paint = table.Column<bool>(type: "bit", nullable: false),
                    quality_id = table.Column<short>(type: "smallint", nullable: true),
                    is_active = table.Column<bool>(type: "bit", nullable: false),
                    name = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_skin", x => x.id);
                    table.ForeignKey(
                        name: "FK_skin_quality_quality_id",
                        column: x => x.quality_id,
                        principalTable: "quality",
                        principalColumn: "id");
                });

            migrationBuilder.CreateTable(
                name: "ItemSkins",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    item_id = table.Column<long>(type: "bigint", nullable: false),
                    skin_id = table.Column<long>(type: "bigint", nullable: false),
                    grade_id = table.Column<long>(type: "bigint", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ItemSkins", x => x.id);
                    table.ForeignKey(
                        name: "FK_ItemSkins_grade_grade_id",
                        column: x => x.grade_id,
                        principalTable: "grade",
                        principalColumn: "id");
                    table.ForeignKey(
                        name: "FK_ItemSkins_item_item_id",
                        column: x => x.item_id,
                        principalTable: "item",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ItemSkins_skin_skin_id",
                        column: x => x.skin_id,
                        principalTable: "skin",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_product_paint_id",
                table: "product",
                column: "paint_id");

            migrationBuilder.CreateIndex(
                name: "IX_product_quality_id",
                table: "product",
                column: "quality_id");

            migrationBuilder.CreateIndex(
                name: "IX_product_sheen_id",
                table: "product",
                column: "sheen_id");

            migrationBuilder.CreateIndex(
                name: "IX_item_class_id",
                table: "item",
                column: "class_id");

            migrationBuilder.CreateIndex(
                name: "IX_item_slot_id",
                table: "item",
                column: "slot_id");

            migrationBuilder.CreateIndex(
                name: "IX_ItemSkins_grade_id",
                table: "ItemSkins",
                column: "grade_id");

            migrationBuilder.CreateIndex(
                name: "IX_ItemSkins_item_id",
                table: "ItemSkins",
                column: "item_id");

            migrationBuilder.CreateIndex(
                name: "IX_ItemSkins_skin_id",
                table: "ItemSkins",
                column: "skin_id");

            migrationBuilder.CreateIndex(
                name: "IX_skin_quality_id",
                table: "skin",
                column: "quality_id");

            migrationBuilder.AddForeignKey(
                name: "FK_item_class_class_id",
                table: "item",
                column: "class_id",
                principalTable: "class",
                principalColumn: "id");

            migrationBuilder.AddForeignKey(
                name: "FK_item_slot_slot_id",
                table: "item",
                column: "slot_id",
                principalTable: "slot",
                principalColumn: "id");

            migrationBuilder.AddForeignKey(
                name: "FK_product_paint_paint_id",
                table: "product",
                column: "paint_id",
                principalTable: "paint",
                principalColumn: "id");

            migrationBuilder.AddForeignKey(
                name: "FK_product_quality_quality_id",
                table: "product",
                column: "quality_id",
                principalTable: "quality",
                principalColumn: "id");

            migrationBuilder.AddForeignKey(
                name: "FK_product_sheen_sheen_id",
                table: "product",
                column: "sheen_id",
                principalTable: "sheen",
                principalColumn: "id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_item_class_class_id",
                table: "item");

            migrationBuilder.DropForeignKey(
                name: "FK_item_slot_slot_id",
                table: "item");

            migrationBuilder.DropForeignKey(
                name: "FK_product_paint_paint_id",
                table: "product");

            migrationBuilder.DropForeignKey(
                name: "FK_product_quality_quality_id",
                table: "product");

            migrationBuilder.DropForeignKey(
                name: "FK_product_sheen_sheen_id",
                table: "product");

            migrationBuilder.DropTable(
                name: "class");

            migrationBuilder.DropTable(
                name: "ItemSkins");

            migrationBuilder.DropTable(
                name: "paint");

            migrationBuilder.DropTable(
                name: "sheen");

            migrationBuilder.DropTable(
                name: "slot");

            migrationBuilder.DropTable(
                name: "grade");

            migrationBuilder.DropTable(
                name: "skin");

            migrationBuilder.DropTable(
                name: "quality");

            migrationBuilder.DropIndex(
                name: "IX_product_paint_id",
                table: "product");

            migrationBuilder.DropIndex(
                name: "IX_product_quality_id",
                table: "product");

            migrationBuilder.DropIndex(
                name: "IX_product_sheen_id",
                table: "product");

            migrationBuilder.DropIndex(
                name: "IX_item_class_id",
                table: "item");

            migrationBuilder.DropIndex(
                name: "IX_item_slot_id",
                table: "item");

            migrationBuilder.DropColumn(
                name: "is_strange",
                table: "product");

            migrationBuilder.DropColumn(
                name: "paint_id",
                table: "product");

            migrationBuilder.DropColumn(
                name: "sheen_id",
                table: "product");

            migrationBuilder.DropColumn(
                name: "current_stock",
                table: "item");

            migrationBuilder.DropColumn(
                name: "rating",
                table: "item");

            migrationBuilder.DropColumn(
                name: "trades_count",
                table: "item");

            migrationBuilder.AlterColumn<long>(
                name: "slot_id",
                table: "item",
                type: "bigint",
                nullable: false,
                defaultValue: 0L,
                oldClrType: typeof(long),
                oldType: "bigint",
                oldNullable: true);

            migrationBuilder.AlterColumn<long>(
                name: "class_id",
                table: "item",
                type: "bigint",
                nullable: false,
                defaultValue: 0L,
                oldClrType: typeof(long),
                oldType: "bigint",
                oldNullable: true);
        }
    }
}
