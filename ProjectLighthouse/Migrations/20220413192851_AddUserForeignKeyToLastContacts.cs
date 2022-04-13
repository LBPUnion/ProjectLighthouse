using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;
using LBPUnion.ProjectLighthouse;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

#nullable disable

namespace ProjectLighthouse.Migrations
{
    [DbContext(typeof(Database))]
    [Migration("20220413192851_AddUserForeignKeyToLastContacts")]
    public partial class AddUserForeignKeyToLastContacts : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // We have to delete the previous last contact information here, since we cannot guarantee that it exists.
            // There's no reliance on this information for long-term usage anyways.
            // See https://github.com/LBPUnion/ProjectLighthouse/issues/247 for more information
            migrationBuilder.Sql("DELETE FROM LastContacts;");

            migrationBuilder.AlterColumn<int>(
                name: "UserId",
                table: "LastContacts",
                type: "int",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "int")
                .OldAnnotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn);

            migrationBuilder.AddForeignKey(
                name: "FK_LastContacts_Users_UserId",
                table: "LastContacts",
                column: "UserId",
                principalTable: "Users",
                principalColumn: "UserId",
                onDelete: ReferentialAction.Cascade);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_LastContacts_Users_UserId",
                table: "LastContacts");

            migrationBuilder.AlterColumn<int>(
                name: "UserId",
                table: "LastContacts",
                type: "int",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "int")
                .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn);
        }
    }
}
