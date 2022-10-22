using LBPUnion.ProjectLighthouse;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ProjectLighthouse.Migrations
{
    [DbContext(typeof(Database))]
    [Migration("20221021184305_AddActActionNews")]
    public partial class AddStreamNews : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Activity",
                columns: table => new {
                    ActivityId = table.Column<int>(type: "int", nullable: false).Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    Category = table.Column<int>(type: "tinyint", nullable: false),
                    Timestamp = table.Column<long>(type: "bigint", nullable: false),
                    DestinationId = table.Column<int>(type: "int", nullable: false),
                    ActionCollection = table.Column<string>(type: "text", nullable: false),
                    ActorCollection = table.Column<string>(type: "text", nullable: false),
                },
                constraints: table => {
                    table.PrimaryKey("PK_Activity", x => x.ActivityId);
                }
            );
            migrationBuilder.CreateTable(
                name: "ACTActionCollection",
                columns: table => new {
                    ActionId = table.Column<int>(type: "int", nullable: false).Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    ActorId = table.Column<int>(type: "int", nullable: false),
                    ObjectId = table.Column<int>(type: "int", nullable: false),
                    ActionType = table.Column<int>(type: "tinyint", nullable: false),
                    ActionTimestamp = table.Column<long>(type: "bigint", nullable: false),
                    Interaction = table.Column<int>(type: "int", nullable: false),
                    Interaction2 = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table => {
                    table.PrimaryKey("PK_ACTActionCollection", x => x.ActionId);
                    table.ForeignKey(
                        name: "FK_ACTActionCollection_Users_UserId",
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
                name: "ACTActionCollection");
            migrationBuilder.DropTable(
                name: "News");
        }
    }
}