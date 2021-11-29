using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ProjectLighthouse.Migrations
{
    public partial class AddPasswordResetRequiredToUser : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "PasswordResetRequired",
                table: "Users",
                type: "tinyint(1)",
                nullable: false,
                defaultValue: false);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "PasswordResetRequired",
                table: "Users");
        }
    }
}
