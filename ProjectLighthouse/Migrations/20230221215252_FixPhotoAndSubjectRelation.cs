using LBPUnion.ProjectLighthouse.Database;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ProjectLighthouse.Migrations
{
    [DbContext(typeof(DatabaseContext))]
    [Migration("20230221215252_FixPhotoAndSubjectRelation")]
    public partial class FixPhotoAndSubjectRelation : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "PhotoId",
                table: "PhotoSubjects",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.Sql(
                "UPDATE PhotoSubjects as ps inner join Photos as p on find_in_set(ps.PhotoSubjectId, p.PhotoSubjectCollection) SET ps.PhotoId = p.PhotoId");

            // Delete unused PhotoSubjects otherwise foreign key constraint will fail
            migrationBuilder.Sql("DELETE from PhotoSubjects where PhotoId = 0");

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
                onDelete: ReferentialAction.Cascade);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
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
        }
    }
}
