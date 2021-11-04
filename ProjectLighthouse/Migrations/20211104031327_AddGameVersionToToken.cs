using Microsoft.EntityFrameworkCore.Migrations;

namespace ProjectLighthouse.Migrations
{
    public partial class AddGameVersionToToken : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("DELETE FROM Tokens;"); // Wipe all old tokens
            
            migrationBuilder.AddColumn<int>(
                name: "GameVersion",
                table: "Tokens",
                type: "int",
                nullable: false,
                defaultValue: 3);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "GameVersion",
                table: "Tokens");
        }
    }
}
