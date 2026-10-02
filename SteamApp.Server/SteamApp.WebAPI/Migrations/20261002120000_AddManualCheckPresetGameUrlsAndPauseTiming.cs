using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using SteamApp.Infrastructure.Context;

#nullable disable

namespace SteamApp.WebAPI.Migrations;

[DbContext(typeof(ApplicationDbContext))]
[Migration("20261002120000_AddManualCheckPresetGameUrlsAndPauseTiming")]
public sealed class AddManualCheckPresetGameUrlsAndPauseTiming : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.AddColumn<long>(
            name: "accumulated_paused_milliseconds",
            table: "manual_check_run",
            type: "bigint",
            nullable: false,
            defaultValue: 0L);

        migrationBuilder.AddColumn<DateTime>(
            name: "paused_at_utc",
            table: "manual_check_run",
            type: "datetime2",
            nullable: true);

        migrationBuilder.CreateTable(
            name: "manual_check_preset_game_url",
            columns: table => new
            {
                manual_check_preset_id = table.Column<long>(type: "bigint", nullable: false),
                game_url_id = table.Column<long>(type: "bigint", nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey(
                    "PK_manual_check_preset_game_url",
                    x => new { x.manual_check_preset_id, x.game_url_id });
                table.ForeignKey(
                    name: "FK_manual_check_preset_game_url_game_url_game_url_id",
                    column: x => x.game_url_id,
                    principalTable: "game_url",
                    principalColumn: "id",
                    onDelete: ReferentialAction.Cascade);
                table.ForeignKey(
                    name: "FK_manual_check_preset_game_url_manual_check_preset_manual_check_preset_id",
                    column: x => x.manual_check_preset_id,
                    principalTable: "manual_check_preset",
                    principalColumn: "id",
                    onDelete: ReferentialAction.Cascade);
            });

        migrationBuilder.CreateIndex(
            name: "IX_manual_check_preset_game_url_game_url_id",
            table: "manual_check_preset_game_url",
            column: "game_url_id");

        migrationBuilder.Sql(
            """
            INSERT INTO [manual_check_preset_game_url] ([manual_check_preset_id], [game_url_id])
            SELECT [preset].[id], [url].[id]
            FROM [manual_check_preset] AS [preset]
            INNER JOIN [game_url] AS [url] ON [url].[game_id] = [preset].[game_id]
            WHERE [url].[scraping_mode_id] = 1;
            """);
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropTable(
            name: "manual_check_preset_game_url");

        migrationBuilder.DropColumn(
            name: "accumulated_paused_milliseconds",
            table: "manual_check_run");

        migrationBuilder.DropColumn(
            name: "paused_at_utc",
            table: "manual_check_run");
    }
}
