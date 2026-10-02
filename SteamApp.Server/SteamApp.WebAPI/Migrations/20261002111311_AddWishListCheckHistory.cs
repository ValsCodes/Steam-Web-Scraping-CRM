using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SteamApp.WebAPI.Migrations
{
    /// <inheritdoc />
    public partial class AddWishListCheckHistory : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "wish_list_check_history",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    wish_list_id = table.Column<long>(type: "bigint", nullable: false),
                    user_id = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: true),
                    game_name = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: false),
                    source = table.Column<int>(type: "int", nullable: false),
                    status = table.Column<int>(type: "int", nullable: false),
                    target_price = table.Column<double>(type: "float", nullable: true),
                    current_price = table.Column<double>(type: "float", nullable: true),
                    is_price_reached = table.Column<bool>(type: "bit", nullable: true),
                    requested_at_utc = table.Column<DateTime>(type: "datetime2", nullable: false),
                    started_at_utc = table.Column<DateTime>(type: "datetime2", nullable: false),
                    completed_at_utc = table.Column<DateTime>(type: "datetime2", nullable: true),
                    correlation_id = table.Column<string>(type: "nvarchar(128)", maxLength: 128, nullable: false),
                    error_code = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    error_text = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_wish_list_check_history", x => x.id);
                    table.ForeignKey(
                        name: "FK_wish_list_check_history_AspNetUsers_user_id",
                        column: x => x.user_id,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_wish_list_check_history_wish_list_wish_list_id",
                        column: x => x.wish_list_id,
                        principalTable: "wish_list",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_wish_list_check_history_user_id",
                table: "wish_list_check_history",
                column: "user_id");

            migrationBuilder.CreateIndex(
                name: "IX_wish_list_check_history_user_id_wish_list_id_started_at_utc",
                table: "wish_list_check_history",
                columns: new[] { "user_id", "wish_list_id", "started_at_utc" });

            migrationBuilder.CreateIndex(
                name: "IX_wish_list_check_history_wish_list_id",
                table: "wish_list_check_history",
                column: "wish_list_id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "wish_list_check_history");
        }
    }
}
