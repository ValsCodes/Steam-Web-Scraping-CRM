using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using SteamApp.Infrastructure.Context;

#nullable disable

namespace SteamApp.WebAPI.Migrations;

[DbContext(typeof(ApplicationDbContext))]
[Migration("20261007073000_ActiveSessionAutomationLimitsAndGlobalCatalog")]
public sealed class ActiveSessionAutomationLimitsAndGlobalCatalog : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropIndex(
            name: "IX_manual_check_preset_game_id_name",
            table: "manual_check_preset");
        migrationBuilder.DropIndex(
            name: "IX_automatic_queue_definition_user_id_name",
            table: "automatic_queue_definition");

        migrationBuilder.AddColumn<int>(
            name: "pause_reason",
            table: "manual_check_run",
            type: "int",
            nullable: true);
        migrationBuilder.AddColumn<string>(
            name: "user_id",
            table: "manual_check_run",
            type: "nvarchar(450)",
            maxLength: 450,
            nullable: true);
        migrationBuilder.AddColumn<string>(
            name: "user_id",
            table: "manual_check_preset",
            type: "nvarchar(450)",
            maxLength: 450,
            nullable: true);
        migrationBuilder.AddColumn<int>(
            name: "pause_reason",
            table: "automatic_queue_run",
            type: "int",
            nullable: true);
        migrationBuilder.AddColumn<string>(
            name: "target_resource_type",
            table: "feedback_request",
            type: "nvarchar(50)",
            maxLength: 50,
            nullable: true);
        migrationBuilder.AddColumn<long>(
            name: "target_resource_id",
            table: "feedback_request",
            type: "bigint",
            nullable: true);
        migrationBuilder.AlterColumn<string>(
            name: "user_id",
            table: "automatic_queue_definition",
            type: "nvarchar(450)",
            maxLength: 450,
            nullable: true,
            oldClrType: typeof(string),
            oldType: "nvarchar(450)",
            oldMaxLength: 450);

        migrationBuilder.Sql(
            """
            EXEC(N'
            UPDATE manual_check_run
            SET user_id = game.user_id
            FROM manual_check_run
            INNER JOIN game ON game.id = manual_check_run.game_id;

            IF EXISTS (SELECT 1 FROM manual_check_run WHERE user_id IS NULL)
                THROW 51000, ''Cannot globalize catalog resources because one or more manual-check runs could not be assigned to an owner.'', 1;
            ');
            """);

        migrationBuilder.AlterColumn<string>(
            name: "user_id",
            table: "manual_check_run",
            type: "nvarchar(450)",
            maxLength: 450,
            nullable: false,
            oldClrType: typeof(string),
            oldType: "nvarchar(450)",
            oldMaxLength: 450,
            oldNullable: true);

        migrationBuilder.CreateTable(
            name: "automation_policy",
            columns: table => new
            {
                id = table.Column<int>(type: "int", nullable: false),
                non_admin_limit_seconds = table.Column<int>(type: "int", nullable: false),
                usage_reset_at_utc = table.Column<DateTime>(type: "datetime2", nullable: true),
                last_modified_by_user_id = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: true),
                last_modified_at_utc = table.Column<DateTime>(type: "datetime2", nullable: false),
                last_reset_by_user_id = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: true),
                last_reset_at_utc = table.Column<DateTime>(type: "datetime2", nullable: true),
                row_version = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_automation_policy", x => x.id);
                table.CheckConstraint("CK_automation_policy_limit", "[non_admin_limit_seconds] >= 0 AND [non_admin_limit_seconds] <= 86400");
                table.CheckConstraint("CK_automation_policy_singleton", "[id] = 1");
            });
        migrationBuilder.CreateTable(
            name: "session_presence_lease",
            columns: table => new
            {
                user_id = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: false),
                tab_id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                updated_at_utc = table.Column<DateTime>(type: "datetime2", nullable: false),
                expires_at_utc = table.Column<DateTime>(type: "datetime2", nullable: false)
            },
            constraints: table => table.PrimaryKey("PK_session_presence_lease", x => x.tab_id));
        migrationBuilder.CreateTable(
            name: "automation_usage_interval",
            columns: table => new
            {
                id = table.Column<long>(type: "bigint", nullable: false)
                    .Annotation("SqlServer:Identity", "1, 1"),
                user_id = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: false),
                manual_check_run_id = table.Column<long>(type: "bigint", nullable: false),
                started_at_utc = table.Column<DateTime>(type: "datetime2", nullable: false),
                ended_at_utc = table.Column<DateTime>(type: "datetime2", nullable: true)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_automation_usage_interval", x => x.id);
                table.ForeignKey(
                    name: "FK_automation_usage_interval_manual_check_run_manual_check_run_id",
                    column: x => x.manual_check_run_id,
                    principalTable: "manual_check_run",
                    principalColumn: "id",
                    onDelete: ReferentialAction.Cascade);
            });

        migrationBuilder.Sql(
            """
            INSERT INTO automation_policy (id, non_admin_limit_seconds, last_modified_at_utc)
            VALUES (1, 900, '2026-10-07T00:00:00.0000000Z');
            """);

        migrationBuilder.Sql(
            """
            EXEC(N'
            UPDATE manual_check_preset SET user_id = NULL;
            UPDATE game_url SET user_id = NULL;
            UPDATE product SET user_id = NULL;
            UPDATE pixel SET user_id = NULL;
            UPDATE tag SET user_id = NULL;
            UPDATE item_group SET user_id = NULL;
            UPDATE game SET user_id = NULL;

            UPDATE manual_check_run
            SET status = 8, pause_reason = 4, paused_at_utc = SYSUTCDATETIME()
            WHERE status IN (1, 2, 7)
              AND NOT EXISTS (
                  SELECT 1
                  FROM AspNetUserRoles user_role
                  INNER JOIN AspNetRoles role ON role.Id = user_role.RoleId
                  WHERE user_role.UserId = manual_check_run.user_id
                    AND role.Name = ''Admin'');

            UPDATE block
            SET status = 3,
                wait_until_utc = NULL
            FROM automatic_queue_run_block block
            INNER JOIN automatic_queue_run run ON run.id = block.automatic_queue_run_id
            WHERE run.status IN (1, 2, 3)
              AND block.status IN (1, 2);

            UPDATE automatic_queue_run
            SET status = 4, pause_reason = 4
            WHERE status IN (1, 2, 3)
              AND NOT EXISTS (
                  SELECT 1
                  FROM AspNetUserRoles user_role
                  INNER JOIN AspNetRoles role ON role.Id = user_role.RoleId
                  WHERE user_role.UserId = automatic_queue_run.user_id
                    AND role.Name = ''Admin'');
            ');
            """);

        migrationBuilder.Sql(
            """
            EXEC(N'CREATE INDEX [IX_manual_check_run_user_id_date] ON [manual_check_run] ([user_id], [date]);');
            EXEC(N'CREATE INDEX [IX_manual_check_run_user_id_status] ON [manual_check_run] ([user_id], [status]);');
            EXEC(N'CREATE INDEX [IX_manual_check_preset_user_id] ON [manual_check_preset] ([user_id]);');
            EXEC(N'CREATE UNIQUE INDEX [IX_manual_check_preset_user_id_game_id_name] ON [manual_check_preset] ([user_id], [game_id], [name]) WHERE [user_id] IS NOT NULL;');
            EXEC(N'CREATE UNIQUE INDEX [IX_manual_check_preset_global_game_id_name] ON [manual_check_preset] ([game_id], [name]) WHERE [user_id] IS NULL;');
            """);
        migrationBuilder.CreateIndex(
            "IX_automatic_queue_definition_user_id_name",
            "automatic_queue_definition",
            new[] { "user_id", "name" },
            unique: true,
            filter: "[user_id] IS NOT NULL");
        migrationBuilder.CreateIndex(
            "IX_automatic_queue_definition_global_name",
            "automatic_queue_definition",
            "name",
            unique: true,
            filter: "[user_id] IS NULL");
        migrationBuilder.CreateIndex(
            "IX_automation_usage_interval_manual_check_run_id",
            "automation_usage_interval",
            "manual_check_run_id",
            unique: true,
            filter: "[ended_at_utc] IS NULL");
        migrationBuilder.CreateIndex("IX_automation_usage_interval_user_id_ended_at_utc", "automation_usage_interval", new[] { "user_id", "ended_at_utc" });
        migrationBuilder.CreateIndex("IX_automation_usage_interval_user_id_started_at_utc", "automation_usage_interval", new[] { "user_id", "started_at_utc" });
        migrationBuilder.CreateIndex("IX_session_presence_lease_expires_at_utc", "session_presence_lease", "expires_at_utc");
        migrationBuilder.CreateIndex("IX_session_presence_lease_user_id_expires_at_utc", "session_presence_lease", new[] { "user_id", "expires_at_utc" });
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropTable("automation_policy");
        migrationBuilder.DropTable("automation_usage_interval");
        migrationBuilder.DropTable("session_presence_lease");
        migrationBuilder.DropIndex("IX_manual_check_run_user_id_date", "manual_check_run");
        migrationBuilder.DropIndex("IX_manual_check_run_user_id_status", "manual_check_run");
        migrationBuilder.DropIndex("IX_manual_check_preset_user_id", "manual_check_preset");
        migrationBuilder.DropIndex("IX_manual_check_preset_user_id_game_id_name", "manual_check_preset");
        migrationBuilder.DropIndex("IX_manual_check_preset_global_game_id_name", "manual_check_preset");
        migrationBuilder.DropIndex("IX_automatic_queue_definition_user_id_name", "automatic_queue_definition");
        migrationBuilder.DropIndex("IX_automatic_queue_definition_global_name", "automatic_queue_definition");
        migrationBuilder.DropColumn("pause_reason", "manual_check_run");
        migrationBuilder.DropColumn("user_id", "manual_check_run");
        migrationBuilder.DropColumn("user_id", "manual_check_preset");
        migrationBuilder.DropColumn("pause_reason", "automatic_queue_run");
        migrationBuilder.DropColumn("target_resource_type", "feedback_request");
        migrationBuilder.DropColumn("target_resource_id", "feedback_request");
        migrationBuilder.AlterColumn<string>(
            name: "user_id",
            table: "automatic_queue_definition",
            type: "nvarchar(450)",
            maxLength: 450,
            nullable: false,
            defaultValue: "",
            oldClrType: typeof(string),
            oldType: "nvarchar(450)",
            oldMaxLength: 450,
            oldNullable: true);
        migrationBuilder.CreateIndex(
            name: "IX_manual_check_preset_game_id_name",
            table: "manual_check_preset",
            columns: new[] { "game_id", "name" },
            unique: true);
        migrationBuilder.CreateIndex(
            name: "IX_automatic_queue_definition_user_id_name",
            table: "automatic_queue_definition",
            columns: new[] { "user_id", "name" },
            unique: true);
    }
}
