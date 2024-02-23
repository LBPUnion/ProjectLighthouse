using LBPUnion.ProjectLighthouse.Database;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ProjectLighthouse.Migrations
{
    [DbContext(typeof(DatabaseContext))]
    [Migration("20240214031744_UpdateTeamPickBoolToTimestamp")]
    public partial class UpdateTeamPickBoolToTimestamp : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<long>(name: "TeamPickTime",
                table: "Slots",
                type: "bigint",
                nullable: false,
                defaultValue: 0L);

            migrationBuilder.Sql("UPDATE `Slots` SET TeamPickTime = 1 WHERE TeamPick = 1");

            migrationBuilder.DropColumn(
                name: "TeamPick",
                table: "Slots");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "TeamPickTime",
                table: "Slots");

            migrationBuilder.AddColumn<bool>(
                name: "TeamPick",
                table: "Slots",
                type: "tinyint(1)",
                nullable: false,
                defaultValue: false);
        }
    }
}
