using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ChatApp.API.Migrations
{
    /// <inheritdoc />
    public partial class UpdatingCascadeActionInChat : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ChatMessages_AspNetUsers_FromUserId",
                table: "ChatMessages");

            migrationBuilder.DropForeignKey(
                name: "FK_ChatMessages_AspNetUsers_ToUserId",
                table: "ChatMessages");

            migrationBuilder.DropForeignKey(
                name: "FK_ChatMessages_Groups_ToGroupId",
                table: "ChatMessages");

            migrationBuilder.AddForeignKey(
                name: "FK_ChatMessages_AspNetUsers_FromUserId",
                table: "ChatMessages",
                column: "FromUserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_ChatMessages_AspNetUsers_ToUserId",
                table: "ChatMessages",
                column: "ToUserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_ChatMessages_Groups_ToGroupId",
                table: "ChatMessages",
                column: "ToGroupId",
                principalTable: "Groups",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ChatMessages_AspNetUsers_FromUserId",
                table: "ChatMessages");

            migrationBuilder.DropForeignKey(
                name: "FK_ChatMessages_AspNetUsers_ToUserId",
                table: "ChatMessages");

            migrationBuilder.DropForeignKey(
                name: "FK_ChatMessages_Groups_ToGroupId",
                table: "ChatMessages");

            migrationBuilder.AddForeignKey(
                name: "FK_ChatMessages_AspNetUsers_FromUserId",
                table: "ChatMessages",
                column: "FromUserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_ChatMessages_AspNetUsers_ToUserId",
                table: "ChatMessages",
                column: "ToUserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_ChatMessages_Groups_ToGroupId",
                table: "ChatMessages",
                column: "ToGroupId",
                principalTable: "Groups",
                principalColumn: "Id");
        }
    }
}
