using Microsoft.EntityFrameworkCore.Migrations;
using LBPUnion.ProjectLighthouse;
using LBPUnion.ProjectLighthouse.Database;
using Microsoft.EntityFrameworkCore.Infrastructure;
namespace ProjectLighthouse.Migrations
{
    [DbContext(typeof(DatabaseContext))]
    [Migration("20211113215128_VisitedLevelPlayCounts")]
    public partial class VisitedLevelPlayCounts : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "PlaysLBP1",
                table: "VisitedLevels",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "PlaysLBP2",
                table: "VisitedLevels",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "PlaysLBP3",
                table: "VisitedLevels",
                type: "int",
                nullable: false,
                defaultValue: 0);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "PlaysLBP1",
                table: "VisitedLevels");

            migrationBuilder.DropColumn(
                name: "PlaysLBP2",
                table: "VisitedLevels");

            migrationBuilder.DropColumn(
                name: "PlaysLBP3",
                table: "VisitedLevels");
        }
    }
}
