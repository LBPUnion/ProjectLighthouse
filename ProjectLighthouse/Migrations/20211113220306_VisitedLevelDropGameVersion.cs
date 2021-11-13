using Microsoft.EntityFrameworkCore.Migrations;

namespace ProjectLighthouse.Migrations
{
    public partial class VisitedLevelDropGameVersion : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "GameVersion",
                table: "VisitedLevels");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "GameVersion",
                table: "VisitedLevels",
                type: "int",
                nullable: false,
                defaultValue: 0);
        }
    }
}
