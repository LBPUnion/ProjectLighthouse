using LBPUnion.ProjectLighthouse;
using LBPUnion.ProjectLighthouse.Database;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ProjectLighthouse.Migrations
{
    [DbContext(typeof(DatabaseContext))]
    [Migration("20220729002704_DeveloperSlots")]
    public partial class DeveloperSlots : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "InternalSlotId",
                table: "Slots",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "Type",
                table: "Slots",
                type: "int",
                defaultValue: 1,
                nullable: false);

            migrationBuilder.AddColumn<int>(
                name: "SlotId",
                table: "Photos",
                type: "int",
                nullable: true);

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

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Photos_Slots_SlotId",
                table: "Photos");

            migrationBuilder.DropIndex(
                name: "IX_Photos_SlotId",
                table: "Photos");

            migrationBuilder.DropColumn(
                name: "InternalSlotId",
                table: "Slots");

            migrationBuilder.DropColumn(
                name: "Type",
                table: "Slots");

            migrationBuilder.DropColumn(
                name: "SlotId",
                table: "Photos");
        }
    }
}
