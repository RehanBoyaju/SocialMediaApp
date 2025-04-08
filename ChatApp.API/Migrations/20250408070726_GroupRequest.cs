using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ChatApp.API.Migrations
{
    /// <inheritdoc />
    public partial class GroupRequest : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "GroupRequest",
                columns: table => new
                {
                    SenderId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    GroupId = table.Column<int>(type: "int", nullable: false),
                    RequestDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    IsAccepted = table.Column<bool>(type: "bit", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_GroupRequest", x => new { x.SenderId, x.GroupId });
                    table.ForeignKey(
                        name: "FK_GroupRequest_AspNetUsers_SenderId",
                        column: x => x.SenderId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_GroupRequest_Groups_GroupId",
                        column: x => x.GroupId,
                        principalTable: "Groups",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateIndex(
                name: "IX_GroupRequest_GroupId",
                table: "GroupRequest",
                column: "GroupId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "GroupRequest");
        }
    }
}
