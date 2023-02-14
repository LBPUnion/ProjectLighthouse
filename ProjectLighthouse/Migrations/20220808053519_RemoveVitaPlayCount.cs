using LBPUnion.ProjectLighthouse;
using LBPUnion.ProjectLighthouse.Database;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ProjectLighthouse.Migrations
{

    [DbContext(typeof(DatabaseContext))]
    [Migration("20220808053519_RemoveVitaPlayCount")]
    public partial class RemoveVitaPlayCount : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("UPDATE Slots SET PlaysLbp2 = PlaysLBP2 + PlaysLBPVita");
            migrationBuilder.Sql("UPDATE Slots SET PlaysLBP2Complete = PlaysLBP2Complete + PlaysLBPVitaComplete");
            migrationBuilder.Sql("UPDATE Slots SET PlaysLBP2Unique = PlaysLBP2Unique + PlaysLBPVitaUnique");
            migrationBuilder.Sql("UPDATE VisitedLevels SET PlaysLBP2 = PlaysLBP2 + PlaysLBPVita");

            migrationBuilder.DropColumn(
                name: "PlaysLBPVita",
                table: "Slots");

            migrationBuilder.DropColumn(
                name: "PlaysLBPVitaComplete",
                table: "Slots");

            migrationBuilder.DropColumn(
                name: "PlaysLBPVitaUnique",
                table: "Slots");

            migrationBuilder.DropColumn(
                name: "PlaysLBPVita",
                table: "VisitedLevels");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
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

            migrationBuilder.AddColumn<int>(
                name: "PlaysLBPVita",
                table: "VisitedLevels",
                type: "int",
                nullable: false,
                defaultValue: 0);
        }
    }
}
