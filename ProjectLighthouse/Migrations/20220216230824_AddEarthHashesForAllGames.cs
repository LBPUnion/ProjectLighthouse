using LBPUnion.ProjectLighthouse;
using LBPUnion.ProjectLighthouse.Database;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ProjectLighthouse.Migrations
{
    [DbContext(typeof(DatabaseContext))]
    [Migration("20220216230824_AddEarthHashesForAllGames")]
    public partial class AddEarthHashesForAllGames : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "PlanetHash",
                table: "Users",
                newName: "PlanetHashLBP2");
            
            migrationBuilder.AddColumn<string>(
                name: "PlanetHashLBP3",
                table: "Users",
                type: "longtext",
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<string>(
                name: "PlanetHashLBPVita",
                table: "Users",
                type: "longtext",
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "PlanetHashLBP2",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "PlanetHashLBP3",
                table: "Users");

            migrationBuilder.RenameColumn(
                name: "PlanetHashLBPVita",
                table: "Users",
                newName: "PlanetHash");
        }
    }
}
