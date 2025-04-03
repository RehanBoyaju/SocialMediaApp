using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ChatApp.API.Migrations
{
    /// <inheritdoc />
    public partial class initial3 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Relationship_AspNetUsers_FriendId",
                table: "Relationship");

            migrationBuilder.DropForeignKey(
                name: "FK_Relationship_AspNetUsers_UserId",
                table: "Relationship");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Relationship",
                table: "Relationship");

            migrationBuilder.RenameTable(
                name: "Relationship",
                newName: "Relationships");

            migrationBuilder.RenameIndex(
                name: "IX_Relationship_UserId",
                table: "Relationships",
                newName: "IX_Relationships_UserId");

            migrationBuilder.RenameIndex(
                name: "IX_Relationship_FriendId",
                table: "Relationships",
                newName: "IX_Relationships_FriendId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Relationships",
                table: "Relationships",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Relationships_AspNetUsers_FriendId",
                table: "Relationships",
                column: "FriendId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Relationships_AspNetUsers_UserId",
                table: "Relationships",
                column: "UserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.NoAction);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Relationships_AspNetUsers_FriendId",
                table: "Relationships");

            migrationBuilder.DropForeignKey(
                name: "FK_Relationships_AspNetUsers_UserId",
                table: "Relationships");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Relationships",
                table: "Relationships");

            migrationBuilder.RenameTable(
                name: "Relationships",
                newName: "Relationship");

            migrationBuilder.RenameIndex(
                name: "IX_Relationships_UserId",
                table: "Relationship",
                newName: "IX_Relationship_UserId");

            migrationBuilder.RenameIndex(
                name: "IX_Relationships_FriendId",
                table: "Relationship",
                newName: "IX_Relationship_FriendId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Relationship",
                table: "Relationship",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Relationship_AspNetUsers_FriendId",
                table: "Relationship",
                column: "FriendId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Relationship_AspNetUsers_UserId",
                table: "Relationship",
                column: "UserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
