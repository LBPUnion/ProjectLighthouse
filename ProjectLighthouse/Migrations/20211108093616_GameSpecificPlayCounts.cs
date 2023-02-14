using Microsoft.EntityFrameworkCore.Migrations;
using LBPUnion.ProjectLighthouse;
using LBPUnion.ProjectLighthouse.Database;
using Microsoft.EntityFrameworkCore.Infrastructure;
namespace ProjectLighthouse.Migrations
{
    [DbContext(typeof(DatabaseContext))]
    [Migration("20211108093616_GameSpecificPlayCounts")]
    public partial class GameSpecificPlayCounts : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "PlaysLBP1",
                table: "Slots",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "PlaysLBP1Complete",
                table: "Slots",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "PlaysLBP1Unique",
                table: "Slots",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "PlaysLBP2",
                table: "Slots",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "PlaysLBP2Complete",
                table: "Slots",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "PlaysLBP2Unique",
                table: "Slots",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "PlaysLBP3",
                table: "Slots",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "PlaysLBP3Complete",
                table: "Slots",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "PlaysLBP3Unique",
                table: "Slots",
                type: "int",
                nullable: false,
                defaultValue: 0);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "PlaysLBP1",
                table: "Slots");

            migrationBuilder.DropColumn(
                name: "PlaysLBP1Complete",
                table: "Slots");

            migrationBuilder.DropColumn(
                name: "PlaysLBP1Unique",
                table: "Slots");

            migrationBuilder.DropColumn(
                name: "PlaysLBP2",
                table: "Slots");

            migrationBuilder.DropColumn(
                name: "PlaysLBP2Complete",
                table: "Slots");

            migrationBuilder.DropColumn(
                name: "PlaysLBP2Unique",
                table: "Slots");

            migrationBuilder.DropColumn(
                name: "PlaysLBP3",
                table: "Slots");

            migrationBuilder.DropColumn(
                name: "PlaysLBP3Complete",
                table: "Slots");

            migrationBuilder.DropColumn(
                name: "PlaysLBP3Unique",
                table: "Slots");

            migrationBuilder.DropColumn(
                name: "Plays",
                table: "Slots");
        }
    }
}
