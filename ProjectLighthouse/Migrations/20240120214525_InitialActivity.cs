using System;
using LBPUnion.ProjectLighthouse.Database;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ProjectLighthouse.Migrations
{
    [DbContext(typeof(DatabaseContext))]
    [Migration("20240120214525_InitialActivity")]
    public partial class InitialActivity : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Activities",
                columns: table => new
                {
                    ActivityId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    Timestamp = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    UserId = table.Column<int>(type: "int", nullable: false),
                    Type = table.Column<int>(type: "int", nullable: false),
                    Discriminator = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    CommentId = table.Column<int>(type: "int", nullable: true),
                    SlotId = table.Column<int>(type: "int", nullable: true),
                    NewsId = table.Column<int>(type: "int", nullable: true),
                    PhotoId = table.Column<int>(type: "int", nullable: true),
                    PlaylistId = table.Column<int>(type: "int", nullable: true),
                    ReviewId = table.Column<int>(type: "int", nullable: true),
                    ScoreId = table.Column<int>(type: "int", nullable: true),
                    TargetUserId = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Activities", x => x.ActivityId);
                    table.ForeignKey(
                        name: "FK_Activities_Comments_CommentId",
                        column: x => x.CommentId,
                        principalTable: "Comments",
                        principalColumn: "CommentId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Activities_Photos_PhotoId",
                        column: x => x.PhotoId,
                        principalTable: "Photos",
                        principalColumn: "PhotoId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Activities_Playlists_PlaylistId",
                        column: x => x.PlaylistId,
                        principalTable: "Playlists",
                        principalColumn: "PlaylistId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Activities_Reviews_ReviewId",
                        column: x => x.ReviewId,
                        principalTable: "Reviews",
                        principalColumn: "ReviewId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Activities_Scores_ScoreId",
                        column: x => x.ScoreId,
                        principalTable: "Scores",
                        principalColumn: "ScoreId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Activities_Slots_SlotId",
                        column: x => x.SlotId,
                        principalTable: "Slots",
                        principalColumn: "SlotId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Activities_Users_TargetUserId",
                        column: x => x.TargetUserId,
                        principalTable: "Users",
                        principalColumn: "UserId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Activities_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "UserId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Activities_WebsiteAnnouncements_NewsId",
                        column: x => x.NewsId,
                        principalTable: "WebsiteAnnouncements",
                        principalColumn: "AnnouncementId",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateIndex(
                name: "IX_Activities_CommentId",
                table: "Activities",
                column: "CommentId");

            migrationBuilder.CreateIndex(
                name: "IX_Activities_NewsId",
                table: "Activities",
                column: "NewsId");

            migrationBuilder.CreateIndex(
                name: "IX_Activities_PhotoId",
                table: "Activities",
                column: "PhotoId");

            migrationBuilder.CreateIndex(
                name: "IX_Activities_PlaylistId",
                table: "Activities",
                column: "PlaylistId");

            migrationBuilder.CreateIndex(
                name: "IX_Activities_ReviewId",
                table: "Activities",
                column: "ReviewId");

            migrationBuilder.CreateIndex(
                name: "IX_Activities_ScoreId",
                table: "Activities",
                column: "ScoreId");

            migrationBuilder.CreateIndex(
                name: "IX_Activities_SlotId",
                table: "Activities",
                column: "SlotId");

            migrationBuilder.CreateIndex(
                name: "IX_Activities_TargetUserId",
                table: "Activities",
                column: "TargetUserId");

            migrationBuilder.CreateIndex(
                name: "IX_Activities_UserId",
                table: "Activities",
                column: "UserId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Activities");
        }
    }
}
