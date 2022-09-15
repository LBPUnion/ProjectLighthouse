using LBPUnion.ProjectLighthouse;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ProjectLighthouse.Migrations
{
    [DbContext(typeof(Database))]
    [Migration("20220915154400_AddisAdventureColumn")]
    public partial class AddisAdventureColumn : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<long>(
                name: "isAdventurePlanet",
                table: "Slots",
                type: "bool",
                nullable: false,
                defaultValue: false);
        }
    }
}
