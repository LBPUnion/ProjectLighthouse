using LBPUnion.ProjectLighthouse.Database;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ProjectLighthouse.Migrations
{
    [DbContext(typeof(DatabaseContext))]
    [Migration("20230319024424_RenameReactionsTableToRatedComments")]
    public partial class RenameReactionsTableToRatedComments : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Reactions_Comments_CommentId",
                table: "Reactions");

            migrationBuilder.DropForeignKey(
                name: "FK_Reactions_Users_UserId",
                table: "Reactions");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Reactions",
                table: "Reactions");

            migrationBuilder.RenameTable(
                name: "Reactions",
                newName: "RatedComments");

            migrationBuilder.RenameIndex(
                name: "IX_Reactions_UserId",
                table: "RatedComments",
                newName: "IX_RatedComments_UserId");

            migrationBuilder.RenameIndex(
                name: "IX_Reactions_CommentId",
                table: "RatedComments",
                newName: "IX_RatedComments_CommentId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_RatedComments",
                table: "RatedComments",
                column: "RatingId");

            migrationBuilder.AddForeignKey(
                name: "FK_RatedComments_Comments_CommentId",
                table: "RatedComments",
                column: "CommentId",
                principalTable: "Comments",
                principalColumn: "CommentId",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_RatedComments_Users_UserId",
                table: "RatedComments",
                column: "UserId",
                principalTable: "Users",
                principalColumn: "UserId",
                onDelete: ReferentialAction.Cascade);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_RatedComments_Comments_CommentId",
                table: "RatedComments");

            migrationBuilder.DropForeignKey(
                name: "FK_RatedComments_Users_UserId",
                table: "RatedComments");

            migrationBuilder.DropPrimaryKey(
                name: "PK_RatedComments",
                table: "RatedComments");

            migrationBuilder.RenameTable(
                name: "RatedComments",
                newName: "Reactions");

            migrationBuilder.RenameIndex(
                name: "IX_RatedComments_UserId",
                table: "Reactions",
                newName: "IX_Reactions_UserId");

            migrationBuilder.RenameIndex(
                name: "IX_RatedComments_CommentId",
                table: "Reactions",
                newName: "IX_Reactions_CommentId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Reactions",
                table: "Reactions",
                column: "RatingId");

            migrationBuilder.AddForeignKey(
                name: "FK_Reactions_Comments_CommentId",
                table: "Reactions",
                column: "CommentId",
                principalTable: "Comments",
                principalColumn: "CommentId",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Reactions_Users_UserId",
                table: "Reactions",
                column: "UserId",
                principalTable: "Users",
                principalColumn: "UserId",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
