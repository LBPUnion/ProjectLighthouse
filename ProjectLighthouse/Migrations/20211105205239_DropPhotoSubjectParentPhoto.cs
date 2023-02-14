using Microsoft.EntityFrameworkCore.Migrations;
using LBPUnion.ProjectLighthouse;
using LBPUnion.ProjectLighthouse.Database;
using Microsoft.EntityFrameworkCore.Infrastructure;
namespace ProjectLighthouse.Migrations
{
    [DbContext(typeof(DatabaseContext))]
    [Migration("20211105205239_DropPhotoSubjectParentPhoto")]
    public partial class DropPhotoSubjectParentPhoto : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_PhotoSubjects_Photos_ParentPhotoId",
                table: "PhotoSubjects");

            migrationBuilder.DropIndex(
                name: "IX_PhotoSubjects_ParentPhotoId",
                table: "PhotoSubjects");

            migrationBuilder.DropColumn(
                name: "ParentPhotoId",
                table: "PhotoSubjects");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "ParentPhotoId",
                table: "PhotoSubjects",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateIndex(
                name: "IX_PhotoSubjects_ParentPhotoId",
                table: "PhotoSubjects",
                column: "ParentPhotoId");

            migrationBuilder.AddForeignKey(
                name: "FK_PhotoSubjects_Photos_ParentPhotoId",
                table: "PhotoSubjects",
                column: "ParentPhotoId",
                principalTable: "Photos",
                principalColumn: "PhotoId",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
