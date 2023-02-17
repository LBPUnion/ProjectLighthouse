using LBPUnion.ProjectLighthouse;
using LBPUnion.ProjectLighthouse.Database;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ProjectLighthouse.Migrations
{
    [DbContext(typeof(DatabaseContext))]
    [Migration("20211214005427_AddUsedBoolToGameToken")]
    public partial class AddUsedBoolToGameToken : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Incompatible with old tokens
            migrationBuilder.Sql("DELETE FROM AuthenticationAttempts");
            migrationBuilder.Sql("DELETE FROM GameTokens");
            
            migrationBuilder.AddColumn<bool>(
                name: "Used",
                table: "GameTokens",
                type: "tinyint(1)",
                nullable: false,
                defaultValue: false);

            migrationBuilder.CreateIndex(
                name: "IX_GameTokens_UserId",
                table: "GameTokens",
                column: "UserId");

            migrationBuilder.AddForeignKey(
                name: "FK_GameTokens_Users_UserId",
                table: "GameTokens",
                column: "UserId",
                principalTable: "Users",
                principalColumn: "UserId",
                onDelete: ReferentialAction.Cascade);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_GameTokens_Users_UserId",
                table: "GameTokens");

            migrationBuilder.DropIndex(
                name: "IX_GameTokens_UserId",
                table: "GameTokens");

            migrationBuilder.DropColumn(
                name: "Used",
                table: "GameTokens");
        }
    }
}
