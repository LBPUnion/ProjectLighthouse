using Microsoft.EntityFrameworkCore.Migrations;
using LBPUnion.ProjectLighthouse;
using LBPUnion.ProjectLighthouse.Database;
using Microsoft.EntityFrameworkCore.Infrastructure;

namespace ProjectLighthouse.Migrations
{
    [DbContext(typeof(DatabaseContext))]
    [Migration("20211105205010_UpdatePhotoAndPhotoSubjectToDoStuffWeirdName")]
    public partial class UpdatePhotoAndPhotoSubjectToDoStuffWeirdName : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_PhotoSubjects_Photos_PhotoId",
                table: "PhotoSubjects");

            migrationBuilder.DropIndex(
                name: "IX_PhotoSubjects_PhotoId",
                table: "PhotoSubjects");

            migrationBuilder.DropColumn(
                name: "PhotoId",
                table: "PhotoSubjects");

            migrationBuilder.AddColumn<string>(
                name: "Bounds",
                table: "PhotoSubjects",
                type: "longtext",
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<int>(
                name: "ParentPhotoId",
                table: "PhotoSubjects",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "UserId",
                table: "PhotoSubjects",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "PhotoSubjectCollection",
                table: "Photos",
                type: "longtext",
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateIndex(
                name: "IX_PhotoSubjects_ParentPhotoId",
                table: "PhotoSubjects",
                column: "ParentPhotoId");

            migrationBuilder.CreateIndex(
                name: "IX_PhotoSubjects_UserId",
                table: "PhotoSubjects",
                column: "UserId");

            migrationBuilder.AddForeignKey(
                name: "FK_PhotoSubjects_Photos_ParentPhotoId",
                table: "PhotoSubjects",
                column: "ParentPhotoId",
                principalTable: "Photos",
                principalColumn: "PhotoId",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_PhotoSubjects_Users_UserId",
                table: "PhotoSubjects",
                column: "UserId",
                principalTable: "Users",
                principalColumn: "UserId",
                onDelete: ReferentialAction.Cascade);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_PhotoSubjects_Photos_ParentPhotoId",
                table: "PhotoSubjects");

            migrationBuilder.DropForeignKey(
                name: "FK_PhotoSubjects_Users_UserId",
                table: "PhotoSubjects");

            migrationBuilder.DropIndex(
                name: "IX_PhotoSubjects_ParentPhotoId",
                table: "PhotoSubjects");

            migrationBuilder.DropIndex(
                name: "IX_PhotoSubjects_UserId",
                table: "PhotoSubjects");

            migrationBuilder.DropColumn(
                name: "Bounds",
                table: "PhotoSubjects");

            migrationBuilder.DropColumn(
                name: "ParentPhotoId",
                table: "PhotoSubjects");

            migrationBuilder.DropColumn(
                name: "UserId",
                table: "PhotoSubjects");

            migrationBuilder.DropColumn(
                name: "PhotoSubjectCollection",
                table: "Photos");

            migrationBuilder.AddColumn<int>(
                name: "PhotoId",
                table: "PhotoSubjects",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_PhotoSubjects_PhotoId",
                table: "PhotoSubjects",
                column: "PhotoId");

            migrationBuilder.AddForeignKey(
                name: "FK_PhotoSubjects_Photos_PhotoId",
                table: "PhotoSubjects",
                column: "PhotoId",
                principalTable: "Photos",
                principalColumn: "PhotoId",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
