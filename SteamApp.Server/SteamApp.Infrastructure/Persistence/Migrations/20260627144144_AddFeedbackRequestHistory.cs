using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SteamApp.Migrations
{
    /// <inheritdoc />
    public partial class AddFeedbackRequestHistory : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "feedback_request_history",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    feedback_request_id = table.Column<long>(type: "bigint", nullable: false),
                    action = table.Column<int>(type: "int", nullable: false),
                    created_at_utc = table.Column<DateTime>(type: "datetime2", nullable: false),
                    previous_type = table.Column<int>(type: "int", nullable: true),
                    new_type = table.Column<int>(type: "int", nullable: true),
                    previous_title = table.Column<string>(type: "nvarchar(140)", maxLength: 140, nullable: true),
                    new_title = table.Column<string>(type: "nvarchar(140)", maxLength: 140, nullable: true),
                    previous_description = table.Column<string>(type: "nvarchar(4000)", maxLength: 4000, nullable: true),
                    new_description = table.Column<string>(type: "nvarchar(4000)", maxLength: 4000, nullable: true),
                    previous_area = table.Column<string>(type: "nvarchar(120)", maxLength: 120, nullable: true),
                    new_area = table.Column<string>(type: "nvarchar(120)", maxLength: 120, nullable: true),
                    previous_status = table.Column<int>(type: "int", nullable: true),
                    new_status = table.Column<int>(type: "int", nullable: true),
                    user_id = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_feedback_request_history", x => x.id);
                    table.ForeignKey(
                        name: "FK_feedback_request_history_AspNetUsers_user_id",
                        column: x => x.user_id,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_feedback_request_history_feedback_request_feedback_request_id",
                        column: x => x.feedback_request_id,
                        principalTable: "feedback_request",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_feedback_request_history_feedback_request_id",
                table: "feedback_request_history",
                column: "feedback_request_id");

            migrationBuilder.CreateIndex(
                name: "IX_feedback_request_history_user_id",
                table: "feedback_request_history",
                column: "user_id");

            migrationBuilder.CreateIndex(
                name: "IX_feedback_request_history_user_id_feedback_request_id_created_at_utc",
                table: "feedback_request_history",
                columns: new[] { "user_id", "feedback_request_id", "created_at_utc" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "feedback_request_history");
        }
    }
}
