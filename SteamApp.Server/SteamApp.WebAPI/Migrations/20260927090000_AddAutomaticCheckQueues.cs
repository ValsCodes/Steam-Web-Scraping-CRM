using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using SteamApp.Infrastructure.Context;

#nullable disable

namespace SteamApp.WebAPI.Migrations;

[DbContext(typeof(ApplicationDbContext))]
[Migration("20260927090000_AddAutomaticCheckQueues")]
public sealed class AddAutomaticCheckQueues : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.CreateTable(
            name: "automatic_queue_definition",
            columns: table => new
            {
                id = table.Column<long>(type: "bigint", nullable: false)
                    .Annotation("SqlServer:Identity", "1, 1"),
                user_id = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: false),
                name = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                created_at_utc = table.Column<DateTime>(type: "datetime2", nullable: false),
                updated_at_utc = table.Column<DateTime>(type: "datetime2", nullable: false)
            },
            constraints: table => table.PrimaryKey("PK_automatic_queue_definition", x => x.id));

        migrationBuilder.CreateTable(
            name: "automatic_queue_block",
            columns: table => new
            {
                id = table.Column<long>(type: "bigint", nullable: false)
                    .Annotation("SqlServer:Identity", "1, 1"),
                automatic_queue_definition_id = table.Column<long>(type: "bigint", nullable: false),
                block_key = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                sort_order = table.Column<int>(type: "int", nullable: false),
                block_type = table.Column<int>(type: "int", nullable: false),
                configuration_json = table.Column<string>(type: "nvarchar(max)", nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_automatic_queue_block", x => x.id);
                table.ForeignKey(
                    name: "FK_automatic_queue_block_automatic_queue_definition_automatic_queue_definition_id",
                    column: x => x.automatic_queue_definition_id,
                    principalTable: "automatic_queue_definition",
                    principalColumn: "id",
                    onDelete: ReferentialAction.Cascade);
            });

        migrationBuilder.CreateTable(
            name: "automatic_queue_run",
            columns: table => new
            {
                id = table.Column<long>(type: "bigint", nullable: false)
                    .Annotation("SqlServer:Identity", "1, 1"),
                automatic_queue_definition_id = table.Column<long>(type: "bigint", nullable: true),
                user_id = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: false),
                queue_name = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                status = table.Column<int>(type: "int", nullable: false),
                current_block_index = table.Column<int>(type: "int", nullable: false),
                date = table.Column<DateTime>(type: "datetime2", nullable: false),
                started_at_utc = table.Column<DateTime>(type: "datetime2", nullable: true),
                completed_at_utc = table.Column<DateTime>(type: "datetime2", nullable: true),
                error_text = table.Column<string>(type: "nvarchar(max)", nullable: true),
                correlation_id = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_automatic_queue_run", x => x.id);
                table.ForeignKey(
                    name: "FK_automatic_queue_run_automatic_queue_definition_automatic_queue_definition_id",
                    column: x => x.automatic_queue_definition_id,
                    principalTable: "automatic_queue_definition",
                    principalColumn: "id",
                    onDelete: ReferentialAction.SetNull);
            });

        migrationBuilder.CreateTable(
            name: "automatic_queue_run_block",
            columns: table => new
            {
                id = table.Column<long>(type: "bigint", nullable: false)
                    .Annotation("SqlServer:Identity", "1, 1"),
                automatic_queue_run_id = table.Column<long>(type: "bigint", nullable: false),
                block_key = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                sort_order = table.Column<int>(type: "int", nullable: false),
                block_type = table.Column<int>(type: "int", nullable: false),
                status = table.Column<int>(type: "int", nullable: false),
                setup_json = table.Column<string>(type: "nvarchar(max)", nullable: false),
                manual_check_run_id = table.Column<long>(type: "bigint", nullable: true),
                wait_until_utc = table.Column<DateTime>(type: "datetime2", nullable: true),
                remaining_delay_seconds = table.Column<int>(type: "int", nullable: true),
                started_at_utc = table.Column<DateTime>(type: "datetime2", nullable: true),
                completed_at_utc = table.Column<DateTime>(type: "datetime2", nullable: true),
                error_text = table.Column<string>(type: "nvarchar(max)", nullable: true)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_automatic_queue_run_block", x => x.id);
                table.ForeignKey(
                    name: "FK_automatic_queue_run_block_automatic_queue_run_automatic_queue_run_id",
                    column: x => x.automatic_queue_run_id,
                    principalTable: "automatic_queue_run",
                    principalColumn: "id",
                    onDelete: ReferentialAction.Cascade);
                table.ForeignKey(
                    name: "FK_automatic_queue_run_block_manual_check_run_manual_check_run_id",
                    column: x => x.manual_check_run_id,
                    principalTable: "manual_check_run",
                    principalColumn: "id",
                    onDelete: ReferentialAction.SetNull);
            });

        migrationBuilder.CreateIndex("IX_automatic_queue_definition_user_id", "automatic_queue_definition", "user_id");
        migrationBuilder.CreateIndex(
            "IX_automatic_queue_definition_user_id_name",
            "automatic_queue_definition",
            new[] { "user_id", "name" },
            unique: true);
        migrationBuilder.CreateIndex(
            "IX_automatic_queue_block_automatic_queue_definition_id_block_key",
            "automatic_queue_block",
            new[] { "automatic_queue_definition_id", "block_key" },
            unique: true);
        migrationBuilder.CreateIndex(
            "IX_automatic_queue_block_automatic_queue_definition_id_sort_order",
            "automatic_queue_block",
            new[] { "automatic_queue_definition_id", "sort_order" },
            unique: true);
        migrationBuilder.CreateIndex(
            "IX_automatic_queue_run_automatic_queue_definition_id",
            "automatic_queue_run",
            "automatic_queue_definition_id",
            unique: true,
            filter: "[automatic_queue_definition_id] IS NOT NULL AND [status] IN (1, 2, 3, 4)");
        migrationBuilder.CreateIndex("IX_automatic_queue_run_status", "automatic_queue_run", "status");
        migrationBuilder.CreateIndex(
            "IX_automatic_queue_run_user_id_date",
            "automatic_queue_run",
            new[] { "user_id", "date" });
        migrationBuilder.CreateIndex(
            "IX_automatic_queue_run_block_automatic_queue_run_id_block_key",
            "automatic_queue_run_block",
            new[] { "automatic_queue_run_id", "block_key" },
            unique: true);
        migrationBuilder.CreateIndex(
            "IX_automatic_queue_run_block_automatic_queue_run_id_sort_order",
            "automatic_queue_run_block",
            new[] { "automatic_queue_run_id", "sort_order" },
            unique: true);
        migrationBuilder.CreateIndex(
            "IX_automatic_queue_run_block_manual_check_run_id",
            "automatic_queue_run_block",
            "manual_check_run_id",
            unique: true,
            filter: "[manual_check_run_id] IS NOT NULL");
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropTable(name: "automatic_queue_block");
        migrationBuilder.DropTable(name: "automatic_queue_run_block");
        migrationBuilder.DropTable(name: "automatic_queue_run");
        migrationBuilder.DropTable(name: "automatic_queue_definition");
    }
}
