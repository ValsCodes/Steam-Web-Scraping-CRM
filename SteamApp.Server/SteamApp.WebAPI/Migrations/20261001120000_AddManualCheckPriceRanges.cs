using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using SteamApp.Infrastructure.Context;

#nullable disable

namespace SteamApp.WebAPI.Migrations;

[DbContext(typeof(ApplicationDbContext))]
[Migration("20261001120000_AddManualCheckPriceRanges")]
public sealed class AddManualCheckPriceRanges : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.AddColumn<int>(
            name: "price_range_mode",
            table: "manual_check_preset",
            type: "int",
            nullable: true);

        migrationBuilder.AddColumn<long>(
            name: "minimum_price_minor_units",
            table: "manual_check_preset",
            type: "bigint",
            nullable: true);

        migrationBuilder.AddColumn<long>(
            name: "maximum_price_minor_units",
            table: "manual_check_preset",
            type: "bigint",
            nullable: true);
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropColumn(
            name: "price_range_mode",
            table: "manual_check_preset");

        migrationBuilder.DropColumn(
            name: "minimum_price_minor_units",
            table: "manual_check_preset");

        migrationBuilder.DropColumn(
            name: "maximum_price_minor_units",
            table: "manual_check_preset");
    }
}
