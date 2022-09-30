using LBPUnion.ProjectLighthouse;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ProjectLighthouse.Migrations
{
    [DbContext(typeof(Database))]
    [Migration("20220928195400_AddStreamNews")]
    public partial class AddStreamNews : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Stream",
                columns: table => new {
                    PostId = table.Column<int>(type: "int", nullable: false),
                    PostType = table.Column<string>(type: "tinytext", nullable: false),
                    Timestamp = table.Column<long>(type: "bigint", nullable: false),
                    ReferencedId = table.Column<int>(type: "int", nullable: false),
                    ActorId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table => {
                    table.PrimaryKey("PK_Stream", x => x.PostId);
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