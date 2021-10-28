using LBPUnion.ProjectLighthouse.Helpers;
using Microsoft.EntityFrameworkCore.Migrations;

namespace ProjectLighthouse.Migrations
{
    public partial class AddSlotFirstUploadedAndLastUpdated : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "Timestamp",
                table: "Slots",
                newName: "LastUpdated");

            migrationBuilder.AddColumn<long>(
                name: "FirstUploaded",
                table: "Slots",
                type: "bigint",
                nullable: false,
                defaultValue: TimeHelper.UnixTimeMilliseconds());
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "FirstUploaded",
                table: "Slots");

            migrationBuilder.RenameColumn(
                name: "LastUpdated",
                table: "Slots",
                newName: "Timestamp");
        }
    }
}
