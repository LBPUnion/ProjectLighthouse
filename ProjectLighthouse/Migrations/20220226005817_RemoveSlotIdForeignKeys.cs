using LBPUnion.ProjectLighthouse;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ProjectLighthouse.Migrations
{
    [DbContext(typeof(Database))]
    [Migration("20220226005817_RemoveSlotIdForeignKeys")]
    public partial class RemoveSlotIdForeignKeys : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_HeartedLevels_Slots_SlotId",
                table: "HeartedLevels");

            migrationBuilder.DropForeignKey(
                name: "FK_Scores_Slots_SlotId",
                table: "Scores");

            migrationBuilder.DropIndex(
                name: "IX_Scores_SlotId",
                table: "Scores");

            migrationBuilder.DropIndex(
                name: "IX_HeartedLevels_SlotId",
                table: "HeartedLevels");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "IX_Scores_SlotId",
                table: "Scores",
                column: "SlotId");

            migrationBuilder.CreateIndex(
                name: "IX_HeartedLevels_SlotId",
                table: "HeartedLevels",
                column: "SlotId");

            migrationBuilder.AddForeignKey(
                name: "FK_HeartedLevels_Slots_SlotId",
                table: "HeartedLevels",
                column: "SlotId",
                principalTable: "Slots",
                principalColumn: "SlotId",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Scores_Slots_SlotId",
                table: "Scores",
                column: "SlotId",
                principalTable: "Slots",
                principalColumn: "SlotId",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
