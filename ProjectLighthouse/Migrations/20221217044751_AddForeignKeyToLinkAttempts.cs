using LBPUnion.ProjectLighthouse;
using LBPUnion.ProjectLighthouse.Database;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ProjectLighthouse.Migrations
{
    [DbContext(typeof(DatabaseContext))]
    [Migration("20221217044751_AddForeignKeyToLinkAttempts")]
    public partial class AddForeignKeyToLinkAttempts : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "IX_PlatformLinkAttempts_UserId",
                table: "PlatformLinkAttempts",
                column: "UserId");

            migrationBuilder.AddForeignKey(
                name: "FK_PlatformLinkAttempts_Users_UserId",
                table: "PlatformLinkAttempts",
                column: "UserId",
                principalTable: "Users",
                principalColumn: "UserId",
                onDelete: ReferentialAction.Cascade);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_PlatformLinkAttempts_Users_UserId",
                table: "PlatformLinkAttempts");

            migrationBuilder.DropIndex(
                name: "IX_PlatformLinkAttempts_UserId",
                table: "PlatformLinkAttempts");
        }
    }
}
