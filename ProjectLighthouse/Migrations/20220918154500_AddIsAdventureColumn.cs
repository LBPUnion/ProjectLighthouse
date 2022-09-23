using LBPUnion.ProjectLighthouse;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ProjectLighthouse.Migrations
{
    [DbContext(typeof(Database))]
    [Migration("20220918154500_AddIsAdventureColumn")]
    public partial class AddisAdventureColumn : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<long>(
                name: "IsAdventurePlanet",
                table: "Slots",
                type: "bool",
                nullable: false,
                defaultValue: false);
        }
    }
}
