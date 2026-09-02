using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ProjectLighthouse.Migrations
{
    /// <inheritdoc />
    public partial class AddUserPinState : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "UserPinProgress",
                columns: table => new
                {
                    UserId = table.Column<int>(type: "int", nullable: false),
                    PinSet = table.Column<int>(type: "int", nullable: false),
                    ProgressType = table.Column<uint>(type: "int unsigned", nullable: false),
                    Value = table.Column<double>(type: "double", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserPinProgress", x => new { x.UserId, x.PinSet, x.ProgressType });
                    table.ForeignKey(
                        name: "FK_UserPinProgress_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "UserId",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "UserProfilePins",
                columns: table => new
                {
                    UserId = table.Column<int>(type: "int", nullable: false),
                    GameVersion = table.Column<int>(type: "int", nullable: false),
                    Pins = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserProfilePins", x => new { x.UserId, x.GameVersion });
                    table.ForeignKey(
                        name: "FK_UserProfilePins_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "UserId",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "UserPinProgress");

            migrationBuilder.DropTable(
                name: "UserProfilePins");
        }
    }
}
