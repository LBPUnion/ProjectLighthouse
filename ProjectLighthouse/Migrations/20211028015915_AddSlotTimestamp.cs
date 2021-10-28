using LBPUnion.ProjectLighthouse.Helpers;
using Microsoft.EntityFrameworkCore.Migrations;

namespace ProjectLighthouse.Migrations
{
    public partial class AddSlotTimestamp : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<long>(
                name: "Timestamp",
                table: "Slots",
                type: "bigint",
                nullable: false,
                defaultValue: TimeHelper.UnixTimeMilliseconds());
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Timestamp",
                table: "Slots");
        }
    }
}
