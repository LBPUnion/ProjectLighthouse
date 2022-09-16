using LBPUnion.ProjectLighthouse;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ProjectLighthouse.Migrations
{
    [DbContext(typeof(Database))]
    [Migration("20220916141401_ScoreboardAdvSlot.cs")]
    public partial class CreateScoreboardAdvSlot : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "AdvSlotId",
                table: "Scores",
                type: "int",
                nullable: true,
                defaultValue: null);
        }
    }
}
