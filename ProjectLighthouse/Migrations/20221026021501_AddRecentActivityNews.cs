using LBPUnion.ProjectLighthouse;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ProjectLighthouse.Migrations
{
    [DbContext(typeof(Database))]
    [Migration("20221026021501_AddRecentActivityNews")]
    public partial class AddRecentActivityNews : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Activity",
                columns: table => new {
                    ActivityId = table.Column<int>(type: "int", nullable: false).Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    TargetType = table.Column<int>(type: "tinyint", nullable: false),
                    TargetId = table.Column<int>(type: "int", nullable: false),
                    UserCollection = table.Column<string>(type: "mediumtext", nullable: false),
                },
                constraints: table => {
                    table.PrimaryKey("PK_Activity", x => x.ActivityId);
                }
            );
            migrationBuilder.CreateTable(
                name: "ActivitySubject",
                columns: table => new {
                    ActionId = table.Column<int>(type: "int", nullable: false).Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    ActionType = table.Column<int>(type: "tinyint", nullable: false),
                    ActionTimestamp = table.Column<long>(type: "bigint", nullable: false),
                    ActorId = table.Column<int>(type: "int", nullable: false),
                    ObjectId = table.Column<int>(type: "int", nullable: false),
                    ObjectType = table.Column<int>(type: "tinyint", nullable: false),
                    Interaction = table.Column<int>(type: "int", nullable: false),
                    Interaction2 = table.Column<long>(type: "bigint", nullable: false)
                },
                constraints: table => {
                    table.PrimaryKey("PK_ActivitySubject", x => x.ActionId);
                    table.ForeignKey(
                        name: "FK_ActivitySubject_Users_UserId",
                        column: a => a.ActorId,
                        principalTable: "Users",
                        principalColumn: "UserId",
                        onDelete: ReferentialAction.Cascade
                    );
                }
            );
            migrationBuilder.CreateTable(
                name: "News",
                columns: table => new {
                    NewsId = table.Column<int>(type: "int", nullable: false).Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
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
                name: "Activity");
            migrationBuilder.DropTable(
                name: "ActivitySubject");
            migrationBuilder.DropTable(
                name: "News");
        }
    }
}