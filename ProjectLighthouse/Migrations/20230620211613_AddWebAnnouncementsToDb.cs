using LBPUnion.ProjectLighthouse.Database;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ProjectLighthouse.Migrations
{
    [DbContext(typeof(DatabaseContext))]
    [Migration("20230620211613_AddWebAnnouncementsToDb")]
    public partial class AddWebAnnouncementsToDb : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "WebsiteAnnouncements",
                columns: table => new
                {
                    AnnouncementId = table.Column<int>(
                            type: "int", 
                            nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    Title = table.Column<string>(
                            type: "longtext", 
                            nullable: false,
                            defaultValue: "")
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Content = table.Column<string>(
                            type: "longtext", 
                            nullable: false,
                            defaultValue: "")
                        .Annotation("MySql:CharSet", "utf8mb4")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_WebsiteAnnouncements", x => x.AnnouncementId);
                })
                .Annotation("MySql:CharSet", "utf8mb4");
        }
        
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "WebsiteAnnouncements");
        }
    }
}
