using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ProjectLighthouse.Migrations
{
    public partial class UserAddDefaultsToNullableStrings : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("UPDATE Slots SET AuthorLabels = \"\" WHERE AuthorLabels IS NULL");
            migrationBuilder.Sql("UPDATE Slots SET LevelType = \"\" WHERE LevelType IS NULL");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {

        }
    }
}
