using Microsoft.EntityFrameworkCore.Migrations;
using LBPUnion.ProjectLighthouse;
using LBPUnion.ProjectLighthouse.Database;
using Microsoft.EntityFrameworkCore.Infrastructure;
#nullable disable

namespace ProjectLighthouse.Migrations
{
    [DbContext(typeof(DatabaseContext))]
    [Migration("20211115050553_UserAddDefaultsToNullableStrings")]
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
