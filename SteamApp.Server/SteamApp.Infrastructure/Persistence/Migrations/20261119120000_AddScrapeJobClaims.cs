using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

namespace SteamApp.Infrastructure.Persistence.Migrations;

[DbContext(typeof(ApplicationDbContext))]
[Migration("20261119120000_AddScrapeJobClaims")]
public sealed class AddScrapeJobClaims : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.AddColumn<Guid>("claim_id", "automated_scrape_history", type: "uniqueidentifier", nullable: true);
        migrationBuilder.AddColumn<DateTime>("claim_expires_at_utc", "automated_scrape_history", type: "datetime2", nullable: true);
        migrationBuilder.CreateIndex(
            name: "IX_automated_scrape_history_status_date",
            table: "automated_scrape_history",
            columns: ["status", "date"]);
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropIndex("IX_automated_scrape_history_status_date", "automated_scrape_history");
        migrationBuilder.DropColumn("claim_expires_at_utc", "automated_scrape_history");
        migrationBuilder.DropColumn("claim_id", "automated_scrape_history");
    }
}
