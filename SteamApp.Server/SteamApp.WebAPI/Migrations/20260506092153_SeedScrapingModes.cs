using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace SteamApp.WebAPI.Migrations
{
    /// <inheritdoc />
    public partial class SeedScrapingModes : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.InsertData(
                table: "scraping_mode",
                columns: new[] { "id", "name" },
                values: new object[,]
                {
                    { 1L, "Manual Batch" },
                    { 2L, "Batch" },
                    { 3L, "Pixel Batch" },
                    { 4L, "Public API" }
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "scraping_mode",
                keyColumn: "id",
                keyValue: 1L);

            migrationBuilder.DeleteData(
                table: "scraping_mode",
                keyColumn: "id",
                keyValue: 2L);

            migrationBuilder.DeleteData(
                table: "scraping_mode",
                keyColumn: "id",
                keyValue: 3L);

            migrationBuilder.DeleteData(
                table: "scraping_mode",
                keyColumn: "id",
                keyValue: 4L);
        }
    }
}
