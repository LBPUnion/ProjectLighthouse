using Microsoft.EntityFrameworkCore.Migrations;
using LBPUnion.ProjectLighthouse;
using Microsoft.EntityFrameworkCore.Infrastructure;
namespace ProjectLighthouse.Migrations
{
    [DbContext(typeof(Database))]
    [Migration("20211102215859_RenameTeamPick")]
    public partial class RenameTeamPick : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "MMPick",
                table: "Slots",
                newName: "TeamPick");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "TeamPick",
                table: "Slots",
                newName: "MMPick");
        }
    }
}
