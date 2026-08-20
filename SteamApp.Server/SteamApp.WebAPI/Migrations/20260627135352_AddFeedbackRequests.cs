using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SteamApp.Migrations
{
    /// <inheritdoc />
    public partial class AddFeedbackRequests : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "feedback_request",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    type = table.Column<int>(type: "int", nullable: false),
                    title = table.Column<string>(type: "nvarchar(140)", maxLength: 140, nullable: false),
                    description = table.Column<string>(type: "nvarchar(4000)", maxLength: 4000, nullable: false),
                    area = table.Column<string>(type: "nvarchar(120)", maxLength: 120, nullable: true),
                    status = table.Column<int>(type: "int", nullable: false),
                    created_at_utc = table.Column<DateTime>(type: "datetime2", nullable: false),
                    updated_at_utc = table.Column<DateTime>(type: "datetime2", nullable: false),
                    status_changed_at_utc = table.Column<DateTime>(type: "datetime2", nullable: false),
                    user_id = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_feedback_request", x => x.id);
                    table.ForeignKey(
                        name: "FK_feedback_request_AspNetUsers_user_id",
                        column: x => x.user_id,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateIndex(
                name: "IX_feedback_request_user_id",
                table: "feedback_request",
                column: "user_id");

            migrationBuilder.CreateIndex(
                name: "IX_feedback_request_user_id_created_at_utc",
                table: "feedback_request",
                columns: new[] { "user_id", "created_at_utc" });

            migrationBuilder.CreateIndex(
                name: "IX_feedback_request_user_id_status",
                table: "feedback_request",
                columns: new[] { "user_id", "status" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "feedback_request");
        }
    }
}
