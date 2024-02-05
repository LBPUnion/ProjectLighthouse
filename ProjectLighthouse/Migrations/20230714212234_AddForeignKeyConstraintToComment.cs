using LBPUnion.ProjectLighthouse.Database;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ProjectLighthouse.Migrations
{
    [DbContext(typeof(DatabaseContext))]
    [Migration("20230714212234_AddForeignKeyConstraintToComment")]
    public partial class AddForeignKeyConstraintToComment : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "TargetId",
                table: "Comments");

            migrationBuilder.CreateIndex(
                name: "IX_Comments_TargetSlotId",
                table: "Comments",
                column: "TargetSlotId");

            migrationBuilder.CreateIndex(
                name: "IX_Comments_TargetUserId",
                table: "Comments",
                column: "TargetUserId");

            migrationBuilder.AddForeignKey(
                name: "FK_Comments_Slots_TargetSlotId",
                table: "Comments",
                column: "TargetSlotId",
                principalTable: "Slots",
                principalColumn: "SlotId",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Comments_Users_TargetUserId",
                table: "Comments",
                column: "TargetUserId",
                principalTable: "Users",
                principalColumn: "UserId",
                onDelete: ReferentialAction.Cascade);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Comments_Slots_TargetSlotId",
                table: "Comments");

            migrationBuilder.DropForeignKey(
                name: "FK_Comments_Users_TargetUserId",
                table: "Comments");

            migrationBuilder.DropIndex(
                name: "IX_Comments_TargetSlotId",
                table: "Comments");

            migrationBuilder.DropIndex(
                name: "IX_Comments_TargetUserId",
                table: "Comments");

            migrationBuilder.AddColumn<int>(
                name: "TargetId",
                table: "Comments",
                type: "int",
                nullable: false,
                defaultValue: 0);
        }
    }
}
