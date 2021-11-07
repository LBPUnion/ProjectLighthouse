using Microsoft.EntityFrameworkCore.Migrations;

namespace ProjectLighthouse.Migrations
{
    public partial class DropPhotoSlot : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Photos_Slots_SlotId",
                table: "Photos");

            migrationBuilder.DropIndex(
                name: "IX_Photos_SlotId",
                table: "Photos");

            migrationBuilder.DropColumn(
                name: "SlotId",
                table: "Photos");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "SlotId",
                table: "Photos",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateIndex(
                name: "IX_Photos_SlotId",
                table: "Photos",
                column: "SlotId");

            migrationBuilder.AddForeignKey(
                name: "FK_Photos_Slots_SlotId",
                table: "Photos",
                column: "SlotId",
                principalTable: "Slots",
                principalColumn: "SlotId",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
