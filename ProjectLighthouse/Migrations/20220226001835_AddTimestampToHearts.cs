using LBPUnion.ProjectLighthouse;
using LBPUnion.ProjectLighthouse.Helpers;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ProjectLighthouse.Migrations
{
    [DbContext(typeof(Database))]
    [Migration("20220226001835_AddTimestampToHearts")]
    public partial class AddTimestampToHearts : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<long>(
                name: "Timestamp",
                table: "QueuedLevels",
                type: "bigint",
                nullable: false,
                defaultValue: TimestampHelper.TimestampMillis);

            migrationBuilder.AddColumn<long>(
                name: "Timestamp",
                table: "HeartedLevels",
                type: "bigint",
                nullable: false,
                defaultValue: TimestampHelper.TimestampMillis);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Timestamp",
                table: "QueuedLevels");

            migrationBuilder.DropColumn(
                name: "Timestamp",
                table: "HeartedLevels");
        }
    }
}
