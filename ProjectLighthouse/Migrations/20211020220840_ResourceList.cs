using Microsoft.EntityFrameworkCore.Migrations;

namespace ProjectLighthouse.Migrations
{
    public partial class ResourceList : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "Resource",
                table: "Slots",
                newName: "ResourceCollection");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "ResourceCollection",
                table: "Slots",
                newName: "Resource");
        }
    }
}
