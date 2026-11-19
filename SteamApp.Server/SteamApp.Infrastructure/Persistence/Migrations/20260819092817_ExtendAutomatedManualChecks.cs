using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace SteamApp.Migrations
{
    /// <inheritdoc />
    public partial class ExtendAutomatedManualChecks : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "cooldown_minutes",
                table: "manual_check_preset",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "cooldown_seconds",
                table: "manual_check_preset",
                type: "int",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "manual_check_condition_operator",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    name = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_manual_check_condition_operator", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "manual_check_criterion",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    manual_check_preset_id = table.Column<long>(type: "bigint", nullable: false),
                    condition_operator_id = table.Column<long>(type: "bigint", nullable: true),
                    sort_order = table.Column<int>(type: "int", nullable: false),
                    name_contains = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    value_contains = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_manual_check_criterion", x => x.id);
                    table.ForeignKey(
                        name: "FK_manual_check_criterion_manual_check_condition_operator_condition_operator_id",
                        column: x => x.condition_operator_id,
                        principalTable: "manual_check_condition_operator",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_manual_check_criterion_manual_check_preset_manual_check_preset_id",
                        column: x => x.manual_check_preset_id,
                        principalTable: "manual_check_preset",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.InsertData(
                table: "manual_check_condition_operator",
                columns: new[] { "id", "name" },
                values: new object[,]
                {
                    { 1L, "AND" },
                    { 2L, "OR" },
                    { 3L, "AND NOT" },
                    { 4L, "OR NOT" },
                    { 5L, "XOR" },
                    { 6L, "NAND" },
                    { 7L, "NOR" }
                });

            migrationBuilder.Sql(
                """
                IF EXISTS (
                    SELECT 1
                    FROM manual_check_preset
                    WHERE ISJSON(criteria_json) <> 1
                )
                    THROW 51000, 'Manual-check criteria backfill stopped because a preset contains invalid JSON.', 1;

                IF EXISTS (
                    SELECT 1
                    FROM manual_check_preset
                    WHERE match_mode NOT IN (1, 2)
                )
                    THROW 51001, 'Manual-check criteria backfill stopped because a preset contains an invalid match mode.', 1;
                """);

            migrationBuilder.Sql(
                """
                INSERT INTO manual_check_criterion
                    (manual_check_preset_id, condition_operator_id, sort_order, name_contains, value_contains)
                SELECT
                    preset.id,
                    CASE
                        WHEN TRY_CONVERT(int, criterion.[key]) = 0 THEN NULL
                        WHEN preset.match_mode = 1 THEN 2
                        ELSE 1
                    END,
                    TRY_CONVERT(int, criterion.[key]),
                    NULLIF(LTRIM(RTRIM(COALESCE(
                        JSON_VALUE(criterion.[value], '$.NameContains'),
                        JSON_VALUE(criterion.[value], '$.nameContains')))), ''),
                    NULLIF(LTRIM(RTRIM(COALESCE(
                        JSON_VALUE(criterion.[value], '$.ValueContains'),
                        JSON_VALUE(criterion.[value], '$.valueContains')))), '')
                FROM manual_check_preset AS preset
                CROSS APPLY OPENJSON(preset.criteria_json) AS criterion;
                """);

            migrationBuilder.DropColumn(
                name: "criteria_json",
                table: "manual_check_preset");

            migrationBuilder.DropColumn(
                name: "match_mode",
                table: "manual_check_preset");

            migrationBuilder.CreateIndex(
                name: "IX_manual_check_criterion_condition_operator_id",
                table: "manual_check_criterion",
                column: "condition_operator_id");

            migrationBuilder.CreateIndex(
                name: "IX_manual_check_criterion_manual_check_preset_id_sort_order",
                table: "manual_check_criterion",
                columns: new[] { "manual_check_preset_id", "sort_order" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "criteria_json",
                table: "manual_check_preset",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "[]");

            migrationBuilder.AddColumn<int>(
                name: "match_mode",
                table: "manual_check_preset",
                type: "int",
                nullable: false,
                defaultValue: 1);

            migrationBuilder.Sql(
                """
                UPDATE preset
                SET
                    criteria_json = (
                        SELECT
                            criterion.name_contains AS [NameContains],
                            criterion.value_contains AS [ValueContains]
                        FROM manual_check_criterion AS criterion
                        WHERE criterion.manual_check_preset_id = preset.id
                        ORDER BY criterion.sort_order
                        FOR JSON PATH
                    ),
                    match_mode = CASE
                        WHEN EXISTS (
                            SELECT 1
                            FROM manual_check_criterion AS criterion
                            WHERE criterion.manual_check_preset_id = preset.id
                              AND criterion.sort_order > 0
                              AND criterion.condition_operator_id <> 2
                        ) THEN 2
                        ELSE 1
                    END
                FROM manual_check_preset AS preset;
                """);

            migrationBuilder.DropTable(
                name: "manual_check_criterion");

            migrationBuilder.DropTable(
                name: "manual_check_condition_operator");

            migrationBuilder.DropColumn(
                name: "cooldown_minutes",
                table: "manual_check_preset");

            migrationBuilder.DropColumn(
                name: "cooldown_seconds",
                table: "manual_check_preset");
        }
    }
}
