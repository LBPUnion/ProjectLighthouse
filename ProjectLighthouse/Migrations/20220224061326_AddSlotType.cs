using LBPUnion.ProjectLighthouse;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ProjectLighthouse.Migrations
{
    [DbContext(typeof(Database))]
    [Migration("20220224061326_AddSlotType")]
    public partial class AddSlotType : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "SlotType",
                table: "Scores",
                type: "int",
                nullable: false,
                defaultValue: 1);

            migrationBuilder.AddColumn<int>(
                name: "SlotType",
                table: "Photos",
                type: "int",
                nullable: false,
                defaultValue: 1);

            migrationBuilder.AddColumn<int>(
                name: "SlotType",
                table: "Comments",
                type: "int",
                nullable: false,
                defaultValue: 1);

            migrationBuilder.AddColumn<int>(name: "SlotType",
                table: "HeartedLevels",
                type: "int",
                nullable: false,
                defaultValue: 1);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "SlotType",
                table: "Scores");

            migrationBuilder.DropColumn(
                name: "SlotType",
                table: "Photos");

            migrationBuilder.DropColumn(
                name: "SlotType",
                table: "Comments");

            migrationBuilder.DropColumn(
                name: "SlotType",
                table: "HeartedLevels");
        }
    }
}
