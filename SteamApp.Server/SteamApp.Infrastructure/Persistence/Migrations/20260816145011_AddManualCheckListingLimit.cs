using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SteamApp.Migrations
{
    /// <inheritdoc />
    public partial class AddManualCheckListingLimit : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "listing_limit",
                table: "manual_check_preset",
                type: "int",
                nullable: false,
                defaultValue: 10);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "listing_limit",
                table: "manual_check_preset");
        }
    }
}
