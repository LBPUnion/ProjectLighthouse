using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ProjectLighthouse.Migrations
{
    public partial class CommentRefactor : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Comments_Slots_TargetId",
                table: "Comments");

            migrationBuilder.DropForeignKey(
                name: "FK_Comments_Users_TargetId",
                table: "Comments");

            migrationBuilder.DropIndex(
                name: "IX_Comments_TargetId",
                table: "Comments");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "IX_Comments_TargetId",
                table: "Comments",
                column: "TargetId");

            migrationBuilder.AddForeignKey(
                name: "FK_Comments_Slots_TargetId",
                table: "Comments",
                column: "TargetId",
                principalTable: "Slots",
                principalColumn: "SlotId",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Comments_Users_TargetId",
                table: "Comments",
                column: "TargetId",
                principalTable: "Users",
                principalColumn: "UserId",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
