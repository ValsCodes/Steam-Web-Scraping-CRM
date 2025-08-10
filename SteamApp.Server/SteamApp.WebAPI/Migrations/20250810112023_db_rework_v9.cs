using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SteamApp.WebAPI.Migrations
{
    /// <inheritdoc />
    public partial class db_rework_v9 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "AddOnTypes",
                columns: table => new
                {
                    id = table.Column<short>(type: "smallint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    is_good = table.Column<bool>(type: "bit", nullable: false),
                    is_team_fortress_paint = table.Column<bool>(type: "bit", nullable: false),
                    name = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AddOnTypes", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "game",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    name = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_game", x => x.id);
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
                name: "class",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    game_id = table.Column<long>(type: "bigint", nullable: false),
                    name = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_class", x => x.id);
                    table.ForeignKey(
                        name: "FK_class_game_game_id",
                        column: x => x.game_id,
                        principalTable: "game",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "game_add_on",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    game_id = table.Column<long>(type: "bigint", nullable: false),
                    add_on_type_id = table.Column<short>(type: "smallint", nullable: false),
                    added_value = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: true),
                    name = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_game_add_on", x => x.id);
                    table.ForeignKey(
                        name: "FK_game_add_on_AddOnTypes_add_on_type_id",
                        column: x => x.add_on_type_id,
                        principalTable: "AddOnTypes",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_game_add_on_game_game_id",
                        column: x => x.game_id,
                        principalTable: "game",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "game_url",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Url = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    game_id = table.Column<long>(type: "bigint", nullable: false),
                    name = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_game_url", x => x.id);
                    table.ForeignKey(
                        name: "FK_game_url_game_game_id",
                        column: x => x.game_id,
                        principalTable: "game",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "grade",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    game_id = table.Column<long>(type: "bigint", nullable: true),
                    name = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_grade", x => x.id);
                    table.ForeignKey(
                        name: "FK_grade_game_game_id",
                        column: x => x.game_id,
                        principalTable: "game",
                        principalColumn: "id");
                });

            migrationBuilder.CreateTable(
                name: "invoice",
                columns: table => new
                {
                    id = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    game_id = table.Column<long>(type: "bigint", nullable: false),
                    transcation_date = table.Column<DateTime>(type: "datetime2", nullable: false),
                    amount = table.Column<double>(type: "float", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_invoice", x => x.id);
                    table.ForeignKey(
                        name: "FK_invoice_game_game_id",
                        column: x => x.game_id,
                        principalTable: "game",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "quality",
                columns: table => new
                {
                    id = table.Column<short>(type: "smallint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    game_id = table.Column<long>(type: "bigint", nullable: false),
                    is_skin_quality = table.Column<bool>(type: "bit", nullable: false),
                    name = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_quality", x => x.id);
                    table.ForeignKey(
                        name: "FK_quality_game_game_id",
                        column: x => x.game_id,
                        principalTable: "game",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "TeamFortressPaintAddOns",
                columns: table => new
                {
                    game_add_on_id = table.Column<long>(type: "bigint", nullable: false),
                    r_value = table.Column<byte>(type: "tinyint", nullable: false),
                    g_value = table.Column<byte>(type: "tinyint", nullable: false),
                    b_value = table.Column<byte>(type: "tinyint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TeamFortressPaintAddOns", x => x.game_add_on_id);
                    table.ForeignKey(
                        name: "FK_TeamFortressPaintAddOns_game_add_on_game_add_on_id",
                        column: x => x.game_add_on_id,
                        principalTable: "game_add_on",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "item",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    is_active = table.Column<bool>(type: "bit", nullable: false),
                    game_id = table.Column<long>(type: "bigint", nullable: false),
                    game_url_id = table.Column<long>(type: "bigint", nullable: false),
                    current_stock = table.Column<int>(type: "int", nullable: true),
                    trades_count = table.Column<int>(type: "int", nullable: true),
                    rating = table.Column<short>(type: "smallint", nullable: true),
                    is_favorite = table.Column<bool>(type: "bit", nullable: false),
                    name = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_item", x => x.id);
                    table.ForeignKey(
                        name: "FK_item_game_game_id",
                        column: x => x.game_id,
                        principalTable: "game",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_item_game_url_game_url_id",
                        column: x => x.game_url_id,
                        principalTable: "game_url",
                        principalColumn: "id");
                });

            migrationBuilder.CreateTable(
                name: "item_classes",
                columns: table => new
                {
                    item_id = table.Column<long>(type: "bigint", nullable: false),
                    class_id = table.Column<long>(type: "bigint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_item_classes", x => new { x.item_id, x.class_id });
                    table.ForeignKey(
                        name: "FK_item_classes_class_class_id",
                        column: x => x.class_id,
                        principalTable: "class",
                        principalColumn: "id");
                    table.ForeignKey(
                        name: "FK_item_classes_item_item_id",
                        column: x => x.item_id,
                        principalTable: "item",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "item_game_add_ons",
                columns: table => new
                {
                    item_id = table.Column<long>(type: "bigint", nullable: false),
                    game_add_on_id = table.Column<long>(type: "bigint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_item_game_add_ons", x => new { x.item_id, x.game_add_on_id });
                    table.ForeignKey(
                        name: "FK_item_game_add_ons_game_add_on_game_add_on_id",
                        column: x => x.game_add_on_id,
                        principalTable: "game_add_on",
                        principalColumn: "id");
                    table.ForeignKey(
                        name: "FK_item_game_add_ons_item_item_id",
                        column: x => x.item_id,
                        principalTable: "item",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "item_qualities",
                columns: table => new
                {
                    item_id = table.Column<long>(type: "bigint", nullable: false),
                    quality_id = table.Column<short>(type: "smallint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_item_qualities", x => new { x.item_id, x.quality_id });
                    table.ForeignKey(
                        name: "FK_item_qualities_item_item_id",
                        column: x => x.item_id,
                        principalTable: "item",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_item_qualities_quality_quality_id",
                        column: x => x.quality_id,
                        principalTable: "quality",
                        principalColumn: "id");
                });

            migrationBuilder.CreateTable(
                name: "item_slots",
                columns: table => new
                {
                    item_id = table.Column<long>(type: "bigint", nullable: false),
                    slot_id = table.Column<long>(type: "bigint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_item_slots", x => new { x.item_id, x.slot_id });
                    table.ForeignKey(
                        name: "FK_item_slots_item_item_id",
                        column: x => x.item_id,
                        principalTable: "item",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_item_slots_slot_slot_id",
                        column: x => x.slot_id,
                        principalTable: "slot",
                        principalColumn: "id");
                });

            migrationBuilder.CreateTable(
                name: "skin",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    item_id = table.Column<long>(type: "bigint", nullable: false),
                    quality_id = table.Column<short>(type: "smallint", nullable: true),
                    is_active = table.Column<bool>(type: "bit", nullable: false),
                    name = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_skin", x => x.id);
                    table.ForeignKey(
                        name: "FK_skin_item_item_id",
                        column: x => x.item_id,
                        principalTable: "item",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_skin_quality_quality_id",
                        column: x => x.quality_id,
                        principalTable: "quality",
                        principalColumn: "id");
                });

            migrationBuilder.CreateTable(
                name: "team_fortress_item",
                columns: table => new
                {
                    item_id = table.Column<long>(type: "bigint", nullable: false),
                    is_hat = table.Column<bool>(type: "bit", nullable: false),
                    is_weapon = table.Column<bool>(type: "bit", nullable: false),
                    ClassId = table.Column<long>(type: "bigint", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_team_fortress_item", x => x.item_id);
                    table.ForeignKey(
                        name: "FK_team_fortress_item_class_ClassId",
                        column: x => x.ClassId,
                        principalTable: "class",
                        principalColumn: "id");
                    table.ForeignKey(
                        name: "FK_team_fortress_item_item_item_id",
                        column: x => x.item_id,
                        principalTable: "item",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "watch_item",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    item_id = table.Column<long>(type: "bigint", nullable: true),
                    last_check_date = table.Column<DateTime>(type: "datetime2", nullable: false),
                    price = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: true),
                    QualityId = table.Column<short>(type: "smallint", nullable: true),
                    name = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_watch_item", x => x.id);
                    table.ForeignKey(
                        name: "FK_watch_item_item_item_id",
                        column: x => x.item_id,
                        principalTable: "item",
                        principalColumn: "id");
                    table.ForeignKey(
                        name: "FK_watch_item_quality_QualityId",
                        column: x => x.QualityId,
                        principalTable: "quality",
                        principalColumn: "id");
                });

            migrationBuilder.CreateTable(
                name: "target",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    watch_item_id = table.Column<long>(type: "bigint", nullable: false),
                    target_price = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_target", x => x.id);
                    table.ForeignKey(
                        name: "FK_target_watch_item_watch_item_id",
                        column: x => x.watch_item_id,
                        principalTable: "watch_item",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_class_game_id",
                table: "class",
                column: "game_id");

            migrationBuilder.CreateIndex(
                name: "IX_game_add_on_add_on_type_id",
                table: "game_add_on",
                column: "add_on_type_id");

            migrationBuilder.CreateIndex(
                name: "IX_game_add_on_game_id",
                table: "game_add_on",
                column: "game_id");

            migrationBuilder.CreateIndex(
                name: "IX_game_url_game_id",
                table: "game_url",
                column: "game_id");

            migrationBuilder.CreateIndex(
                name: "IX_grade_game_id",
                table: "grade",
                column: "game_id");

            migrationBuilder.CreateIndex(
                name: "IX_invoice_game_id",
                table: "invoice",
                column: "game_id");

            migrationBuilder.CreateIndex(
                name: "IX_item_game_id",
                table: "item",
                column: "game_id");

            migrationBuilder.CreateIndex(
                name: "IX_item_game_url_id",
                table: "item",
                column: "game_url_id");

            migrationBuilder.CreateIndex(
                name: "IX_item_classes_class_id",
                table: "item_classes",
                column: "class_id");

            migrationBuilder.CreateIndex(
                name: "IX_item_game_add_ons_game_add_on_id",
                table: "item_game_add_ons",
                column: "game_add_on_id");

            migrationBuilder.CreateIndex(
                name: "IX_item_qualities_quality_id",
                table: "item_qualities",
                column: "quality_id");

            migrationBuilder.CreateIndex(
                name: "IX_item_slots_slot_id",
                table: "item_slots",
                column: "slot_id");

            migrationBuilder.CreateIndex(
                name: "IX_quality_game_id",
                table: "quality",
                column: "game_id");

            migrationBuilder.CreateIndex(
                name: "IX_skin_item_id",
                table: "skin",
                column: "item_id");

            migrationBuilder.CreateIndex(
                name: "IX_skin_quality_id",
                table: "skin",
                column: "quality_id");

            migrationBuilder.CreateIndex(
                name: "IX_target_watch_item_id",
                table: "target",
                column: "watch_item_id");

            migrationBuilder.CreateIndex(
                name: "IX_team_fortress_item_ClassId",
                table: "team_fortress_item",
                column: "ClassId");

            migrationBuilder.CreateIndex(
                name: "IX_watch_item_item_id",
                table: "watch_item",
                column: "item_id");

            migrationBuilder.CreateIndex(
                name: "IX_watch_item_QualityId",
                table: "watch_item",
                column: "QualityId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "grade");

            migrationBuilder.DropTable(
                name: "invoice");

            migrationBuilder.DropTable(
                name: "item_classes");

            migrationBuilder.DropTable(
                name: "item_game_add_ons");

            migrationBuilder.DropTable(
                name: "item_qualities");

            migrationBuilder.DropTable(
                name: "item_slots");

            migrationBuilder.DropTable(
                name: "skin");

            migrationBuilder.DropTable(
                name: "target");

            migrationBuilder.DropTable(
                name: "team_fortress_item");

            migrationBuilder.DropTable(
                name: "TeamFortressPaintAddOns");

            migrationBuilder.DropTable(
                name: "slot");

            migrationBuilder.DropTable(
                name: "watch_item");

            migrationBuilder.DropTable(
                name: "class");

            migrationBuilder.DropTable(
                name: "game_add_on");

            migrationBuilder.DropTable(
                name: "item");

            migrationBuilder.DropTable(
                name: "quality");

            migrationBuilder.DropTable(
                name: "AddOnTypes");

            migrationBuilder.DropTable(
                name: "game_url");

            migrationBuilder.DropTable(
                name: "game");
        }
    }
}
