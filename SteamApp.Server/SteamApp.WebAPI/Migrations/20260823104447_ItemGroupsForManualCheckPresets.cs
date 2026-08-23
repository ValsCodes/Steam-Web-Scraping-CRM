using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SteamApp.WebAPI.Migrations
{
    /// <inheritdoc />
    public partial class ItemGroupsForManualCheckPresets : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_tag_tag_type_tag_type_id",
                table: "tag");

            migrationBuilder.DropForeignKey(
                name: "FK_tag_type_AspNetUsers_user_id",
                table: "tag_type");

            migrationBuilder.DropForeignKey(
                name: "FK_tag_type_game_game_id",
                table: "tag_type");

            migrationBuilder.DropPrimaryKey(
                name: "PK_tag_type",
                table: "tag_type");

            migrationBuilder.RenameTable(
                name: "tag_type",
                newName: "item_group");

            migrationBuilder.RenameIndex(
                name: "IX_tag_type_user_id_game_id_name",
                table: "item_group",
                newName: "IX_item_group_user_id_game_id_name");

            migrationBuilder.RenameIndex(
                name: "IX_tag_type_user_id",
                table: "item_group",
                newName: "IX_item_group_user_id");

            migrationBuilder.RenameIndex(
                name: "IX_tag_type_game_id",
                table: "item_group",
                newName: "IX_item_group_game_id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_item_group",
                table: "item_group",
                column: "id");

            migrationBuilder.AddForeignKey(
                name: "FK_item_group_AspNetUsers_user_id",
                table: "item_group",
                column: "user_id",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "FK_item_group_game_game_id",
                table: "item_group",
                column: "game_id",
                principalTable: "game",
                principalColumn: "id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.RenameColumn(
                name: "tag_type_id",
                table: "tag",
                newName: "item_group_id");

            migrationBuilder.RenameIndex(
                name: "IX_tag_tag_type_id",
                table: "tag",
                newName: "IX_tag_item_group_id");

            migrationBuilder.AddColumn<long>(
                name: "item_group_id",
                table: "manual_check_preset",
                type: "bigint",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_manual_check_preset_item_group_id",
                table: "manual_check_preset",
                column: "item_group_id");

            migrationBuilder.AddForeignKey(
                name: "FK_manual_check_preset_item_group_item_group_id",
                table: "manual_check_preset",
                column: "item_group_id",
                principalTable: "item_group",
                principalColumn: "id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_tag_item_group_item_group_id",
                table: "tag",
                column: "item_group_id",
                principalTable: "item_group",
                principalColumn: "id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_manual_check_preset_item_group_item_group_id",
                table: "manual_check_preset");

            migrationBuilder.DropForeignKey(
                name: "FK_tag_item_group_item_group_id",
                table: "tag");

            migrationBuilder.DropForeignKey(
                name: "FK_item_group_AspNetUsers_user_id",
                table: "item_group");

            migrationBuilder.DropForeignKey(
                name: "FK_item_group_game_game_id",
                table: "item_group");

            migrationBuilder.DropPrimaryKey(
                name: "PK_item_group",
                table: "item_group");

            migrationBuilder.DropIndex(
                name: "IX_manual_check_preset_item_group_id",
                table: "manual_check_preset");

            migrationBuilder.DropColumn(
                name: "item_group_id",
                table: "manual_check_preset");

            migrationBuilder.RenameColumn(
                name: "item_group_id",
                table: "tag",
                newName: "tag_type_id");

            migrationBuilder.RenameIndex(
                name: "IX_tag_item_group_id",
                table: "tag",
                newName: "IX_tag_tag_type_id");

            migrationBuilder.RenameIndex(
                name: "IX_item_group_user_id_game_id_name",
                table: "item_group",
                newName: "IX_tag_type_user_id_game_id_name");

            migrationBuilder.RenameIndex(
                name: "IX_item_group_user_id",
                table: "item_group",
                newName: "IX_tag_type_user_id");

            migrationBuilder.RenameIndex(
                name: "IX_item_group_game_id",
                table: "item_group",
                newName: "IX_tag_type_game_id");

            migrationBuilder.RenameTable(
                name: "item_group",
                newName: "tag_type");

            migrationBuilder.AddPrimaryKey(
                name: "PK_tag_type",
                table: "tag_type",
                column: "id");

            migrationBuilder.AddForeignKey(
                name: "FK_tag_type_AspNetUsers_user_id",
                table: "tag_type",
                column: "user_id",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "FK_tag_type_game_game_id",
                table: "tag_type",
                column: "game_id",
                principalTable: "game",
                principalColumn: "id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_tag_tag_type_tag_type_id",
                table: "tag",
                column: "tag_type_id",
                principalTable: "tag_type",
                principalColumn: "id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
