using Microsoft.EntityFrameworkCore.Migrations;

namespace ProjectLighthouse.Migrations
{
    public partial class AddUserLocationToToken : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "UserLocation",
                table: "Tokens",
                type: "longtext",
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "UserLocation",
                table: "Tokens");
        }
    }
}
