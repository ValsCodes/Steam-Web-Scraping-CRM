using System;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using SteamApp.Infrastructure.Context;

#nullable disable

namespace SteamApp.Migrations
{
    /// <inheritdoc />
    [DbContext(typeof(ApplicationDbContext))]
    [Migration("20260705120000_AddScrapeHistoryJobStatus")]
    public partial class AddScrapeHistoryJobStatus : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "status",
                table: "automated_scrape_history",
                type: "int",
                nullable: false,
                defaultValue: 3);

            migrationBuilder.AddColumn<DateTime>(
                name: "started_at_utc",
                table: "automated_scrape_history",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "completed_at_utc",
                table: "automated_scrape_history",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "correlation_id",
                table: "automated_scrape_history",
                type: "nvarchar(64)",
                maxLength: 64,
                nullable: true);

            migrationBuilder.Sql(
                """
                UPDATE automated_scrape_history
                SET
                    status = CASE WHEN is_have_error = 1 THEN 4 ELSE 3 END,
                    started_at_utc = [date],
                    completed_at_utc = [date]
                """);

            migrationBuilder.CreateIndex(
                name: "IX_automated_scrape_history_user_id_status",
                table: "automated_scrape_history",
                columns: new[] { "user_id", "status" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_automated_scrape_history_user_id_status",
                table: "automated_scrape_history");

            migrationBuilder.DropColumn(
                name: "completed_at_utc",
                table: "automated_scrape_history");

            migrationBuilder.DropColumn(
                name: "correlation_id",
                table: "automated_scrape_history");

            migrationBuilder.DropColumn(
                name: "started_at_utc",
                table: "automated_scrape_history");

            migrationBuilder.DropColumn(
                name: "status",
                table: "automated_scrape_history");
        }
    }
}
