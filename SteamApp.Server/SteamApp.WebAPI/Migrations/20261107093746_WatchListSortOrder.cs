using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SteamApp.WebAPI.Migrations
{
    /// <inheritdoc />
    public partial class WatchListSortOrder : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "sort_order",
                table: "watch_list",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.Sql(
                """
                WITH ranked AS
                (
                    SELECT id,
                           ROW_NUMBER() OVER (PARTITION BY user_id ORDER BY id) - 1 AS new_sort_order
                    FROM watch_list
                )
                UPDATE watch_list
                SET sort_order = ranked.new_sort_order
                FROM watch_list
                INNER JOIN ranked ON ranked.id = watch_list.id;
                """);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "sort_order",
                table: "watch_list");
        }
    }
}
