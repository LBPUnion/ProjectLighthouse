using Microsoft.EntityFrameworkCore.Migrations;
using LBPUnion.ProjectLighthouse;
using LBPUnion.ProjectLighthouse.Database;
using Microsoft.EntityFrameworkCore.Infrastructure;

#nullable disable

namespace ProjectLighthouse.Migrations
{
    [DbContext(typeof(DatabaseContext))]
    [Migration("20211115052941_SlotAddLbpVitaPlays")]
    public partial class SlotAddLbpVitaPlays : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "PlaysLBPVita",
                table: "VisitedLevels",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "PlaysLBPVita",
                table: "Slots",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "PlaysLBPVitaComplete",
                table: "Slots",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "PlaysLBPVitaUnique",
                table: "Slots",
                type: "int",
                nullable: false,
                defaultValue: 0);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "PlaysLBPVita",
                table: "VisitedLevels");

            migrationBuilder.DropColumn(
                name: "PlaysLBPVita",
                table: "Slots");

            migrationBuilder.DropColumn(
                name: "PlaysLBPVitaComplete",
                table: "Slots");

            migrationBuilder.DropColumn(
                name: "PlaysLBPVitaUnique",
                table: "Slots");
        }
    }
}
