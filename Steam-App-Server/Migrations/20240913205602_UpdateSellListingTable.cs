using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SteamAppServer.Migrations
{
    /// <inheritdoc />
    public partial class UpdateSellListingTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "seld_price",
                table: "sell_listing",
                newName: "sold_price");

            migrationBuilder.AlterColumn<DateTime>(
                name: "date_sold",
                table: "sell_listing",
                type: "datetime2",
                nullable: true,
                oldClrType: typeof(DateTime),
                oldType: "datetime2");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "sold_price",
                table: "sell_listing",
                newName: "seld_price");

            migrationBuilder.AlterColumn<DateTime>(
                name: "date_sold",
                table: "sell_listing",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified),
                oldClrType: typeof(DateTime),
                oldType: "datetime2",
                oldNullable: true);
        }
    }
}
