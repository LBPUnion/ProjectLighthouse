using Microsoft.EntityFrameworkCore.Migrations;
using LBPUnion.ProjectLighthouse;
using Microsoft.EntityFrameworkCore.Infrastructure;
#nullable disable

namespace ProjectLighthouse.Migrations
{
    [DbContext(typeof(Database))]
    [Migration("20220716234844_RemovedAPIKeyEnabled")]
    public partial class RemovedAPIKeyEnabled : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Enabled",
                table: "APIKeys");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "Enabled",
                table: "APIKeys",
                type: "tinyint(1)",
                nullable: false,
                defaultValue: false);
        }
    }
}
