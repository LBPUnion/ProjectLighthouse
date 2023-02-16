using System;
using LBPUnion.ProjectLighthouse;
using LBPUnion.ProjectLighthouse.Database;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ProjectLighthouse.Migrations
{
    [DbContext(typeof(DatabaseContext))]
    [Migration("20220729182709_AddExpiryTimesToTokens")]
    public partial class AddExpiryTimesToTokens : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Remove existing tokens
            migrationBuilder.Sql("DELETE FROM GameTokens;");
            migrationBuilder.Sql("DELETE FROM WebTokens;");
            migrationBuilder.Sql("DELETE FROM EmailSetTokens;");
            migrationBuilder.Sql("DELETE FROM EmailVerificationTokens;");
            migrationBuilder.Sql("DELETE FROM PasswordResetTokens;");
            migrationBuilder.Sql("DELETE FROM RegistrationTokens;");

            migrationBuilder.AddColumn<DateTime>(
                name: "ExpiresAt",
                table: "WebTokens",
                type: "datetime(6)",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<DateTime>(
                name: "ExpiresAt",
                table: "GameTokens",
                type: "datetime(6)",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<DateTime>(
                name: "ExpiresAt",
                table: "EmailVerificationTokens",
                type: "datetime(6)",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<DateTime>(
                name: "ExpiresAt",
                table: "EmailSetTokens",
                type: "datetime(6)",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ExpiresAt",
                table: "WebTokens");

            migrationBuilder.DropColumn(
                name: "ExpiresAt",
                table: "RegistrationTokens");

            migrationBuilder.DropColumn(
                name: "ExpiresAt",
                table: "PasswordResetTokens");

            migrationBuilder.DropColumn(
                name: "ExpiresAt",
                table: "GameTokens");

            migrationBuilder.DropColumn(
                name: "ExpiresAt",
                table: "EmailVerificationTokens");

            migrationBuilder.DropColumn(
                name: "ExpiresAt",
                table: "EmailSetTokens");
        }
    }
}
