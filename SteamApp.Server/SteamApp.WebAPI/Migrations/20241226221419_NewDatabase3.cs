using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SteamApp.Migrations;

/// <inheritdoc />
public partial class NewDatabase3 : Migration
{
    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.CreateTable(
            name: "quality",
            columns: table => new
            {
                id = table.Column<short>(type: "smallint", nullable: false)
                    .Annotation("SqlServer:Identity", "1, 1"),
                name = table.Column<string>(type: "nvarchar(max)", nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_quality", x => x.id);
            });

        migrationBuilder.CreateTable(
            name: "sell_listing",
            columns: table => new
            {
                id = table.Column<long>(type: "bigint", nullable: false)
                    .Annotation("SqlServer:Identity", "1, 1"),
                name = table.Column<string>(type: "nvarchar(max)", nullable: true),
                quality_id = table.Column<short>(type: "smallint", nullable: true),
                desciption = table.Column<string>(type: "nvarchar(max)", nullable: true),
                date_bought = table.Column<DateTime>(type: "datetime2", nullable: false),
                date_sold = table.Column<DateTime>(type: "datetime2", nullable: true),
                cost_price = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                t_sell_price_1 = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                t_sell_price_2 = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: true),
                t_sell_price_3 = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: true),
                t_sell_price_4 = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: true),
                sold_price = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: true),
                is_hat = table.Column<bool>(type: "bit", nullable: false),
                is_weapon = table.Column<bool>(type: "bit", nullable: false),
                is_sold = table.Column<bool>(type: "bit", nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_sell_listing", x => x.id);
            });
    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropTable(
            name: "quality");

        migrationBuilder.DropTable(
            name: "sell_listing");
    }
}
