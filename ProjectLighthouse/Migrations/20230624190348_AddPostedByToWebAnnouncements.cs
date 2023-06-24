using LBPUnion.ProjectLighthouse.Database;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ProjectLighthouse.Migrations
{
    [DbContext(typeof(DatabaseContext))]
    [Migration("20230624190348_AddPostedByToWebAnnouncements")]
    public partial class AddPostedByToWebAnnouncements : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "PublisherId",
                table: "WebsiteAnnouncements",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateIndex(
                name: "IX_WebsiteAnnouncements_PublisherId",
                table: "WebsiteAnnouncements",
                column: "PublisherId");

            migrationBuilder.AddForeignKey(
                name: "FK_WebsiteAnnouncements_Users_PublisherId",
                table: "WebsiteAnnouncements",
                column: "PublisherId",
                principalTable: "Users",
                principalColumn: "UserId",
                onDelete: ReferentialAction.Cascade);
        }
        
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_WebsiteAnnouncements_Users_PublisherId",
                table: "WebsiteAnnouncements");

            migrationBuilder.DropIndex(
                name: "IX_WebsiteAnnouncements_PublisherId",
                table: "WebsiteAnnouncements");

            migrationBuilder.DropColumn(
                name: "PublisherId",
                table: "WebsiteAnnouncements");
        }
    }
}
