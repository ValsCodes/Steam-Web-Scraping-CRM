using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SteamApp.Migrations
{
    /// <inheritdoc />
    public partial class AddManualChecks : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "manual_check_preset",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    game_id = table.Column<long>(type: "bigint", nullable: false),
                    name = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    match_mode = table.Column<int>(type: "int", nullable: false),
                    criteria_json = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    created_at_utc = table.Column<DateTime>(type: "datetime2", nullable: false),
                    updated_at_utc = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_manual_check_preset", x => x.id);
                    table.ForeignKey(
                        name: "FK_manual_check_preset_game_game_id",
                        column: x => x.game_id,
                        principalTable: "game",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "manual_check_run",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    manual_check_preset_id = table.Column<long>(type: "bigint", nullable: true),
                    game_id = table.Column<long>(type: "bigint", nullable: false),
                    game_url_id = table.Column<long>(type: "bigint", nullable: false),
                    preset_name = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    setup_json = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    results_json = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    total_products = table.Column<int>(type: "int", nullable: false),
                    checked_products = table.Column<int>(type: "int", nullable: false),
                    matched_products = table.Column<int>(type: "int", nullable: false),
                    failed_products = table.Column<int>(type: "int", nullable: false),
                    status = table.Column<int>(type: "int", nullable: false),
                    date = table.Column<DateTime>(type: "datetime2", nullable: false),
                    started_at_utc = table.Column<DateTime>(type: "datetime2", nullable: true),
                    completed_at_utc = table.Column<DateTime>(type: "datetime2", nullable: true),
                    error_text = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    correlation_id = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_manual_check_run", x => x.id);
                    table.ForeignKey(
                        name: "FK_manual_check_run_game_game_id",
                        column: x => x.game_id,
                        principalTable: "game",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_manual_check_run_game_url_game_url_id",
                        column: x => x.game_url_id,
                        principalTable: "game_url",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_manual_check_run_manual_check_preset_manual_check_preset_id",
                        column: x => x.manual_check_preset_id,
                        principalTable: "manual_check_preset",
                        principalColumn: "id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateIndex(
                name: "IX_manual_check_preset_game_id_name",
                table: "manual_check_preset",
                columns: new[] { "game_id", "name" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_manual_check_run_date",
                table: "manual_check_run",
                column: "date");

            migrationBuilder.CreateIndex(
                name: "IX_manual_check_run_game_id",
                table: "manual_check_run",
                column: "game_id");

            migrationBuilder.CreateIndex(
                name: "IX_manual_check_run_game_url_id",
                table: "manual_check_run",
                column: "game_url_id");

            migrationBuilder.CreateIndex(
                name: "IX_manual_check_run_manual_check_preset_id",
                table: "manual_check_run",
                column: "manual_check_preset_id");

            migrationBuilder.CreateIndex(
                name: "IX_manual_check_run_status",
                table: "manual_check_run",
                column: "status");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(name: "manual_check_run");
            migrationBuilder.DropTable(name: "manual_check_preset");
        }
    }
}
