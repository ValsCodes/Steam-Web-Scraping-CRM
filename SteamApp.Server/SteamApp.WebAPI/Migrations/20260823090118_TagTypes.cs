using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SteamApp.WebAPI.Migrations
{
    /// <inheritdoc />
    public partial class TagTypes : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<long>(
                name: "tag_type_id",
                table: "tag",
                type: "bigint",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "tag_type",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    game_id = table.Column<long>(type: "bigint", nullable: false),
                    name = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    user_id = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_tag_type", x => x.id);
                    table.ForeignKey(
                        name: "FK_tag_type_AspNetUsers_user_id",
                        column: x => x.user_id,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_tag_type_game_game_id",
                        column: x => x.game_id,
                        principalTable: "game",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_tag_tag_type_id",
                table: "tag",
                column: "tag_type_id");

            migrationBuilder.CreateIndex(
                name: "IX_tag_type_game_id",
                table: "tag_type",
                column: "game_id");

            migrationBuilder.CreateIndex(
                name: "IX_tag_type_user_id",
                table: "tag_type",
                column: "user_id");

            migrationBuilder.CreateIndex(
                name: "IX_tag_type_user_id_game_id_name",
                table: "tag_type",
                columns: new[] { "user_id", "game_id", "name" },
                unique: true,
                filter: "[user_id] IS NOT NULL");

            migrationBuilder.AddForeignKey(
                name: "FK_tag_tag_type_tag_type_id",
                table: "tag",
                column: "tag_type_id",
                principalTable: "tag_type",
                principalColumn: "id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_tag_tag_type_tag_type_id",
                table: "tag");

            migrationBuilder.DropTable(
                name: "tag_type");

            migrationBuilder.DropIndex(
                name: "IX_tag_tag_type_id",
                table: "tag");

            migrationBuilder.DropColumn(
                name: "tag_type_id",
                table: "tag");
        }
    }
}
