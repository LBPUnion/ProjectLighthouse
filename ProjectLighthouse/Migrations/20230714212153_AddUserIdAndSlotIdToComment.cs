using LBPUnion.ProjectLighthouse.Database;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ProjectLighthouse.Migrations
{
    [DbContext(typeof(DatabaseContext))]
    [Migration("20230714212153_AddUserIdAndSlotIdToComment")]
    public partial class AddUserIdAndSlotIdToComment : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "TargetSlotId",
                table: "Comments",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "TargetUserId",
                table: "Comments",
                type: "int",
                nullable: true);

            // Set SlotId and UserId to null if there isn't an existing slot or user
            migrationBuilder.Sql("UPDATE Comments AS c INNER JOIN Users AS u ON c.TargetId = u.UserId AND c.Type = 0 SET c.TargetUserId = u.UserId");
            migrationBuilder.Sql("UPDATE Comments AS c INNER JOIN Slots AS s ON c.TargetId = s.SlotId AND c.Type = 1 SET c.TargetSlotId = s.SlotId");

            // Delete rows that have null SlotIds or UserIds
            migrationBuilder.Sql("DELETE FROM Comments WHERE TargetUserId IS NULL AND TargetSlotId IS NULL");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "TargetSlotId",
                table: "Comments");

            migrationBuilder.DropColumn(
                name: "TargetUserId",
                table: "Comments");
        }
    }
}
