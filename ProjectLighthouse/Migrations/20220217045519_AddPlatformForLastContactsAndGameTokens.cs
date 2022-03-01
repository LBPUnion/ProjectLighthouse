using LBPUnion.ProjectLighthouse;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ProjectLighthouse.Migrations
{
    [DbContext(typeof(Database))]
    [Migration("20220217045519_AddPlatformForLastContactsAndGameTokens")]
    public partial class AddPlatformForLastContactsAndGameTokens : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "Platform",
                table: "LastContacts",
                type: "int",
                nullable: false,
                defaultValue: -1);

            migrationBuilder.AddColumn<int>(
                name: "Platform",
                table: "GameTokens",
                type: "int",
                nullable: false,
                defaultValue: -1);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Platform",
                table: "LastContacts");

            migrationBuilder.DropColumn(
                name: "Platform",
                table: "GameTokens");
        }
    }
}
