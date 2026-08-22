using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SteamApp.WebAPI.Migrations
{
    /// <inheritdoc />
    public partial class AddManualCheckCriterionGroups : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "close_group_count",
                table: "manual_check_criterion",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "open_group_count",
                table: "manual_check_criterion",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddCheckConstraint(
                name: "CK_manual_check_criterion_close_group_count",
                table: "manual_check_criterion",
                sql: "[close_group_count] >= 0 AND [close_group_count] <= 25");

            migrationBuilder.AddCheckConstraint(
                name: "CK_manual_check_criterion_open_group_count",
                table: "manual_check_criterion",
                sql: "[open_group_count] >= 0 AND [open_group_count] <= 25");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropCheckConstraint(
                name: "CK_manual_check_criterion_close_group_count",
                table: "manual_check_criterion");

            migrationBuilder.DropCheckConstraint(
                name: "CK_manual_check_criterion_open_group_count",
                table: "manual_check_criterion");

            migrationBuilder.DropColumn(
                name: "close_group_count",
                table: "manual_check_criterion");

            migrationBuilder.DropColumn(
                name: "open_group_count",
                table: "manual_check_criterion");
        }
    }
}
