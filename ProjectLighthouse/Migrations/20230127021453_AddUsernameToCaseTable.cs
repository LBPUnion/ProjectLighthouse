using LBPUnion.ProjectLighthouse;
using LBPUnion.ProjectLighthouse.Database;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ProjectLighthouse.Migrations
{
    [DbContext(typeof(DatabaseContext))]
    [Migration("20230127021453_AddUsernameToCaseTable")]
    public partial class AddUsernameToCaseTable : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "CreatorUsername",
                table: "Cases",
                type: "longtext",
                nullable: false,
                defaultValue: "")
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<string>(
                name: "DismisserUsername",
                table: "Cases",
                type: "longtext",
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.Sql("UPDATE Cases INNER JOIN Users ON Cases.CreatorId = Users.UserId SET Cases.CreatorUsername = Users.Username WHERE Cases.CreatorUsername = '';");
            migrationBuilder.Sql("UPDATE Cases INNER JOIN Users ON Cases.DismisserId = Users.UserId SET Cases.DismisserUsername = Users.Username WHERE Cases.DismisserUsername is NULL;");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CreatorUsername",
                table: "Cases");

            migrationBuilder.DropColumn(
                name: "DismisserUsername",
                table: "Cases");
        }
    }
}
