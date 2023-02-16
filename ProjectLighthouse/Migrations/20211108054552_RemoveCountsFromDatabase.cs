using Microsoft.EntityFrameworkCore.Migrations;
using LBPUnion.ProjectLighthouse;
using LBPUnion.ProjectLighthouse.Database;
using Microsoft.EntityFrameworkCore.Infrastructure;
namespace ProjectLighthouse.Migrations
{
    [DbContext(typeof(DatabaseContext))]
    [Migration("20211108054552_RemoveCountsFromDatabase")]
    public partial class RemoveCountsFromDatabase : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CommentCount",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "FavouriteSlotCount",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "FavouriteUserCount",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "HeartCount",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "Lists",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "LolCatFtwCount",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "ReviewCount",
                table: "Users");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "CommentCount",
                table: "Users",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "FavouriteSlotCount",
                table: "Users",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "FavouriteUserCount",
                table: "Users",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "HeartCount",
                table: "Users",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "Lists",
                table: "Users",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "LolCatFtwCount",
                table: "Users",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "ReviewCount",
                table: "Users",
                type: "int",
                nullable: false,
                defaultValue: 0);
        }
    }
}
