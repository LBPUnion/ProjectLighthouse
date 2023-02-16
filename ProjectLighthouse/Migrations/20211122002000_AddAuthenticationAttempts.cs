using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;
using LBPUnion.ProjectLighthouse;
using LBPUnion.ProjectLighthouse.Database;
using Microsoft.EntityFrameworkCore.Infrastructure;

#nullable disable

namespace ProjectLighthouse.Migrations
{
    [DbContext(typeof(DatabaseContext))]
    [Migration("20211122002000_AddAuthenticationAttempts")]
    public partial class AddAuthenticationAttempts : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "Approved",
                table: "GameTokens",
                type: "tinyint(1)",
                nullable: false,
                defaultValue: false);

            migrationBuilder.CreateTable(
                name: "AuthenticationAttempts",
                columns: table => new
                {
                    AuthenticationAttemptId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    Timestamp = table.Column<long>(type: "bigint", nullable: false),
                    Platform = table.Column<int>(type: "int", nullable: false),
                    IPAddress = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    GameTokenId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AuthenticationAttempts", x => x.AuthenticationAttemptId);
                    table.ForeignKey(
                        name: "FK_AuthenticationAttempts_GameTokens_GameTokenId",
                        column: x => x.GameTokenId,
                        principalTable: "GameTokens",
                        principalColumn: "TokenId",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateIndex(
                name: "IX_AuthenticationAttempts_GameTokenId",
                table: "AuthenticationAttempts",
                column: "GameTokenId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AuthenticationAttempts");

            migrationBuilder.DropColumn(
                name: "Approved",
                table: "GameTokens");
        }
    }
}
