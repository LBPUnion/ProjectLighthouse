using LBPUnion.ProjectLighthouse;
using LBPUnion.ProjectLighthouse.Database;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ProjectLighthouse.Migrations
{
    [DbContext(typeof(DatabaseContext))]
    [Migration("20221217002014_ReworkGameTokens")]
    public partial class ReworkGameTokens : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AuthenticationAttempts");

            migrationBuilder.DropColumn(
                name: "ApprovedIPAddress",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "Approved",
                table: "GameTokens");

            migrationBuilder.DropColumn(
                name: "Used",
                table: "GameTokens");

            migrationBuilder.AddColumn<ulong>(
                name: "LinkedPsnId",
                table: "Users",
                type: "bigint unsigned",
                nullable: false,
                defaultValue: 0ul);

            migrationBuilder.AddColumn<ulong>(
                name: "LinkedRpcnId",
                table: "Users",
                type: "bigint unsigned",
                nullable: false,
                defaultValue: 0ul);

            migrationBuilder.AddColumn<string>(
                name: "TicketHash",
                table: "GameTokens",
                type: "longtext",
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "LinkedPsnId",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "LinkedRpcnId",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "TicketHash",
                table: "GameTokens");

            migrationBuilder.AddColumn<string>(
                name: "ApprovedIPAddress",
                table: "Users",
                type: "longtext",
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<bool>(
                name: "Approved",
                table: "GameTokens",
                type: "tinyint(1)",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "Used",
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
                    GameTokenId = table.Column<int>(type: "int", nullable: false),
                    IPAddress = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Platform = table.Column<int>(type: "int", nullable: false),
                    Timestamp = table.Column<long>(type: "bigint", nullable: false)
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
    }
}
