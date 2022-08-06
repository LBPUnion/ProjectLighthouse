using LBPUnion.ProjectLighthouse;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ProjectLighthouse.Migrations
{
    [DbContext(typeof(Database))]
    [Migration("20220805234507_AddModNotesToCase")]
    public partial class AddModNotesToCase : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "Description",
                table: "Cases",
                newName: "Reason");

            migrationBuilder.AddColumn<string>(
                name: "ModeratorNotes",
                table: "Cases",
                type: "longtext",
                nullable: false)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<bool>(
                name: "Processed",
                table: "Cases",
                type: "tinyint(1)",
                nullable: false,
                defaultValue: false);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ModeratorNotes",
                table: "Cases");

            migrationBuilder.DropColumn(
                name: "Processed",
                table: "Cases");

            migrationBuilder.RenameColumn(
                name: "Reason",
                table: "Cases",
                newName: "Description");
        }
    }
}
