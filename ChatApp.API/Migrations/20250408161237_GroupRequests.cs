using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ChatApp.API.Migrations
{
    /// <inheritdoc />
    public partial class GroupRequests : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_GroupRequest_AspNetUsers_SenderId",
                table: "GroupRequest");

            migrationBuilder.DropForeignKey(
                name: "FK_GroupRequest_Groups_GroupId",
                table: "GroupRequest");

            migrationBuilder.DropPrimaryKey(
                name: "PK_GroupRequest",
                table: "GroupRequest");

            migrationBuilder.RenameTable(
                name: "GroupRequest",
                newName: "GroupRequests");

            migrationBuilder.RenameIndex(
                name: "IX_GroupRequest_GroupId",
                table: "GroupRequests",
                newName: "IX_GroupRequests_GroupId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_GroupRequests",
                table: "GroupRequests",
                columns: new[] { "SenderId", "GroupId" });

            migrationBuilder.AddForeignKey(
                name: "FK_GroupRequests_AspNetUsers_SenderId",
                table: "GroupRequests",
                column: "SenderId",
                principalTable: "AspNetUsers",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_GroupRequests_Groups_GroupId",
                table: "GroupRequests",
                column: "GroupId",
                principalTable: "Groups",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_GroupRequests_AspNetUsers_SenderId",
                table: "GroupRequests");

            migrationBuilder.DropForeignKey(
                name: "FK_GroupRequests_Groups_GroupId",
                table: "GroupRequests");

            migrationBuilder.DropPrimaryKey(
                name: "PK_GroupRequests",
                table: "GroupRequests");

            migrationBuilder.RenameTable(
                name: "GroupRequests",
                newName: "GroupRequest");

            migrationBuilder.RenameIndex(
                name: "IX_GroupRequests_GroupId",
                table: "GroupRequest",
                newName: "IX_GroupRequest_GroupId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_GroupRequest",
                table: "GroupRequest",
                columns: new[] { "SenderId", "GroupId" });

            migrationBuilder.AddForeignKey(
                name: "FK_GroupRequest_AspNetUsers_SenderId",
                table: "GroupRequest",
                column: "SenderId",
                principalTable: "AspNetUsers",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_GroupRequest_Groups_GroupId",
                table: "GroupRequest",
                column: "GroupId",
                principalTable: "Groups",
                principalColumn: "Id");
        }
    }
}
