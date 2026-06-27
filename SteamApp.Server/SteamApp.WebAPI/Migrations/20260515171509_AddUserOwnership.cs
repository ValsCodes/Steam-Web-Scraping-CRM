using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SteamApp.WebAPI.Migrations
{
    /// <inheritdoc />
    public partial class AddUserOwnership : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "user_id",
                table: "wish_list",
                type: "nvarchar(450)",
                maxLength: 450,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "user_id",
                table: "watch_list",
                type: "nvarchar(450)",
                maxLength: 450,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "user_id",
                table: "tag",
                type: "nvarchar(450)",
                maxLength: 450,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "user_id",
                table: "product",
                type: "nvarchar(450)",
                maxLength: 450,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "user_id",
                table: "pixel",
                type: "nvarchar(450)",
                maxLength: 450,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "user_id",
                table: "game_url",
                type: "nvarchar(450)",
                maxLength: 450,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "user_id",
                table: "game",
                type: "nvarchar(450)",
                maxLength: 450,
                nullable: true);

            migrationBuilder.Sql("""
                DECLARE @SingleOwnerId nvarchar(450);

                SELECT @SingleOwnerId =
                    CASE WHEN COUNT(*) = 1 THEN MAX([Id]) ELSE NULL END
                FROM [AspNetUsers];

                IF @SingleOwnerId IS NOT NULL
                BEGIN
                    UPDATE [game]
                    SET [user_id] = @SingleOwnerId
                    WHERE [user_id] IS NULL;

                    UPDATE [game_url]
                    SET [user_id] = @SingleOwnerId
                    WHERE [user_id] IS NULL;

                    UPDATE [product]
                    SET [user_id] = @SingleOwnerId
                    WHERE [user_id] IS NULL;

                    UPDATE [pixel]
                    SET [user_id] = @SingleOwnerId
                    WHERE [user_id] IS NULL;

                    UPDATE [tag]
                    SET [user_id] = @SingleOwnerId
                    WHERE [user_id] IS NULL;

                    UPDATE [watch_list]
                    SET [user_id] = @SingleOwnerId
                    WHERE [user_id] IS NULL;

                    UPDATE [wish_list]
                    SET [user_id] = @SingleOwnerId
                    WHERE [user_id] IS NULL;
                END
                """);

            migrationBuilder.CreateIndex(
                name: "IX_wish_list_user_id",
                table: "wish_list",
                column: "user_id");

            migrationBuilder.CreateIndex(
                name: "IX_watch_list_user_id",
                table: "watch_list",
                column: "user_id");

            migrationBuilder.CreateIndex(
                name: "IX_tag_user_id",
                table: "tag",
                column: "user_id");

            migrationBuilder.CreateIndex(
                name: "IX_product_user_id",
                table: "product",
                column: "user_id");

            migrationBuilder.CreateIndex(
                name: "IX_pixel_user_id",
                table: "pixel",
                column: "user_id");

            migrationBuilder.CreateIndex(
                name: "IX_game_url_user_id",
                table: "game_url",
                column: "user_id");

            migrationBuilder.CreateIndex(
                name: "IX_game_user_id",
                table: "game",
                column: "user_id");

            migrationBuilder.AddForeignKey(
                name: "FK_game_AspNetUsers_user_id",
                table: "game",
                column: "user_id",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "FK_game_url_AspNetUsers_user_id",
                table: "game_url",
                column: "user_id",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "FK_pixel_AspNetUsers_user_id",
                table: "pixel",
                column: "user_id",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "FK_product_AspNetUsers_user_id",
                table: "product",
                column: "user_id",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "FK_tag_AspNetUsers_user_id",
                table: "tag",
                column: "user_id",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "FK_watch_list_AspNetUsers_user_id",
                table: "watch_list",
                column: "user_id",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "FK_wish_list_AspNetUsers_user_id",
                table: "wish_list",
                column: "user_id",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_game_AspNetUsers_user_id",
                table: "game");

            migrationBuilder.DropForeignKey(
                name: "FK_game_url_AspNetUsers_user_id",
                table: "game_url");

            migrationBuilder.DropForeignKey(
                name: "FK_pixel_AspNetUsers_user_id",
                table: "pixel");

            migrationBuilder.DropForeignKey(
                name: "FK_product_AspNetUsers_user_id",
                table: "product");

            migrationBuilder.DropForeignKey(
                name: "FK_tag_AspNetUsers_user_id",
                table: "tag");

            migrationBuilder.DropForeignKey(
                name: "FK_watch_list_AspNetUsers_user_id",
                table: "watch_list");

            migrationBuilder.DropForeignKey(
                name: "FK_wish_list_AspNetUsers_user_id",
                table: "wish_list");

            migrationBuilder.DropIndex(
                name: "IX_wish_list_user_id",
                table: "wish_list");

            migrationBuilder.DropIndex(
                name: "IX_watch_list_user_id",
                table: "watch_list");

            migrationBuilder.DropIndex(
                name: "IX_tag_user_id",
                table: "tag");

            migrationBuilder.DropIndex(
                name: "IX_product_user_id",
                table: "product");

            migrationBuilder.DropIndex(
                name: "IX_pixel_user_id",
                table: "pixel");

            migrationBuilder.DropIndex(
                name: "IX_game_url_user_id",
                table: "game_url");

            migrationBuilder.DropIndex(
                name: "IX_game_user_id",
                table: "game");

            migrationBuilder.DropColumn(
                name: "user_id",
                table: "wish_list");

            migrationBuilder.DropColumn(
                name: "user_id",
                table: "watch_list");

            migrationBuilder.DropColumn(
                name: "user_id",
                table: "tag");

            migrationBuilder.DropColumn(
                name: "user_id",
                table: "product");

            migrationBuilder.DropColumn(
                name: "user_id",
                table: "pixel");

            migrationBuilder.DropColumn(
                name: "user_id",
                table: "game_url");

            migrationBuilder.DropColumn(
                name: "user_id",
                table: "game");
        }
    }
}
