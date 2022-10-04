using LBPUnion.ProjectLighthouse;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ProjectLighthouse.Migrations
{
    [DbContext(typeof(Database))]
    [Migration("20221003230800_AddStreamNews")]
    public partial class AddStreamNews : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Stream",
                columns: table => new {
                    ActivityId = table.Column<int>(type: "int", nullable: false),
                    Timestamp = table.Column<long>(type: "bigint", nullable: false),
                    ActorId = table.Column<int>(type: "int", nullable: false),
                    TargetId = table.Column<int>(type: "int", nullable: false),
                    ObjectCollection = table.Column<int>(type: "int", nullable: false),
                    EventTypeCollection = table.Column<string>(type: "text", nullable: false),
                    EventTimestampCollection = table.Column<string>(type: "text", nullable: false),
                    InteractCollection = table.Column<string>(type: "text", nullable: false) // This won't be used yet but will be in the future.
                },
                constraints: table => {
                    table.PrimaryKey("PK_Stream", x => x.ActivityId);
                    table.ForeignKey(
                        name: "FK_Stream_Users_UserId",
                        column: s => s.ActorId,
                        principalTable: "Users",
                        principalColumn: "UserId",
                        onDelete: ReferentialAction.Cascade);
                }
            );
            migrationBuilder.CreateTable(
                name: "News",
                columns: table => new {
                    NewsId = table.Column<int>(type: "int", nullable: false),
                    CreatorId = table.Column<int>(type: "int", nullable: false),
                    Category = table.Column<string>(type: "tinytext", nullable: false),
                    Title = table.Column<string>(type: "tinytext", nullable: false),
                    Summary = table.Column<string>(type: "text", nullable: false),
                    Text = table.Column<string>(type: "longtext", nullable: false),
                    Timestamp = table.Column<long>(type: "bigint", nullable: false),
                    ImageAlign = table.Column<string>(type: "tinytext", nullable: false),
                    ImageHash = table.Column<string>(type: "longtext", nullable: false)
                },
                constraints: table => {
                    table.PrimaryKey("PK_News", x => x.NewsId);
                } 
            );
        }
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Stream");

            migrationBuilder.DropTable(
                name: "News");
        }
    }
}