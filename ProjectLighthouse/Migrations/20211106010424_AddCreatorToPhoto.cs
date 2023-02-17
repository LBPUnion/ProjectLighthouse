using Microsoft.EntityFrameworkCore.Migrations;
using LBPUnion.ProjectLighthouse;
using LBPUnion.ProjectLighthouse.Database;
using Microsoft.EntityFrameworkCore.Infrastructure;
namespace ProjectLighthouse.Migrations
{
    [DbContext(typeof(DatabaseContext))]
    [Migration("20211106010424_AddCreatorToPhoto")]
    public partial class AddCreatorToPhoto : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Conflicts with old data.
            migrationBuilder.Sql("DELETE FROM Photos;");
            migrationBuilder.Sql("DELETE FROM PhotoSubjects;"); // Also delete PhotoSubjects while we're at it.
            
            migrationBuilder.AddColumn<int>(
                name: "CreatorId",
                table: "Photos",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateIndex(
                name: "IX_Photos_CreatorId",
                table: "Photos",
                column: "CreatorId");

            migrationBuilder.AddForeignKey(
                name: "FK_Photos_Users_CreatorId",
                table: "Photos",
                column: "CreatorId",
                principalTable: "Users",
                principalColumn: "UserId",
                onDelete: ReferentialAction.Cascade);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Photos_Users_CreatorId",
                table: "Photos");

            migrationBuilder.DropIndex(
                name: "IX_Photos_CreatorId",
                table: "Photos");

            migrationBuilder.DropColumn(
                name: "CreatorId",
                table: "Photos");
        }
    }
}
