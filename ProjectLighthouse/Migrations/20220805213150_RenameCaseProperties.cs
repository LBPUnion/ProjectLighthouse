using System;
using LBPUnion.ProjectLighthouse;
using LBPUnion.ProjectLighthouse.Database;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ProjectLighthouse.Migrations
{
    [DbContext(typeof(DatabaseContext))]
    [Migration("20220805213150_RenameCaseProperties")]
    public partial class RenameCaseProperties : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("DELETE FROM Cases;");
            
            migrationBuilder.DropForeignKey(
                name: "FK_Cases_Users_CaseCreatorId",
                table: "Cases");

            migrationBuilder.RenameColumn(
                name: "CaseType",
                table: "Cases",
                newName: "Type");

            migrationBuilder.RenameColumn(
                name: "CaseExpires",
                table: "Cases",
                newName: "ExpiresAt");

            migrationBuilder.RenameColumn(
                name: "CaseDescription",
                table: "Cases",
                newName: "Description");

            migrationBuilder.RenameColumn(
                name: "CaseCreatorId",
                table: "Cases",
                newName: "CreatorId");

            migrationBuilder.RenameColumn(
                name: "CaseCreated",
                table: "Cases",
                newName: "CreatedAt");

            migrationBuilder.RenameIndex(
                name: "FK_Cases_Users_CaseCreatorId",
                table: "Cases",
                newName: "FK_Cases_Users_CreatorId");

            migrationBuilder.AddColumn<DateTime>(
                name: "DismissedAt",
                table: "Cases",
                type: "datetime(6)",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "DismisserId",
                table: "Cases",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Cases_DismisserId",
                table: "Cases",
                column: "DismisserId");

            migrationBuilder.AddForeignKey(
                name: "FK_Cases_Users_CreatorId",
                table: "Cases",
                column: "CreatorId",
                principalTable: "Users",
                principalColumn: "UserId",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Cases_Users_DismisserId",
                table: "Cases",
                column: "DismisserId",
                principalTable: "Users",
                principalColumn: "UserId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Cases_Users_CreatorId",
                table: "Cases");

            migrationBuilder.DropForeignKey(
                name: "FK_Cases_Users_DismisserId",
                table: "Cases");

            migrationBuilder.DropIndex(
                name: "IX_Cases_DismisserId",
                table: "Cases");

            migrationBuilder.DropColumn(
                name: "DismissedAt",
                table: "Cases");

            migrationBuilder.DropColumn(
                name: "DismisserId",
                table: "Cases");

            migrationBuilder.RenameColumn(
                name: "Type",
                table: "Cases",
                newName: "CaseType");

            migrationBuilder.RenameColumn(
                name: "ExpiresAt",
                table: "Cases",
                newName: "CaseExpires");

            migrationBuilder.RenameColumn(
                name: "Description",
                table: "Cases",
                newName: "CaseDescription");

            migrationBuilder.RenameColumn(
                name: "CreatorId",
                table: "Cases",
                newName: "CaseCreatorId");

            migrationBuilder.RenameColumn(
                name: "CreatedAt",
                table: "Cases",
                newName: "CaseCreated");

            migrationBuilder.RenameIndex(
                name: "IX_Cases_CreatorId",
                table: "Cases",
                newName: "IX_Cases_CaseCreatorId");

            migrationBuilder.AddColumn<bool>(
                name: "Banned",
                table: "Users",
                type: "tinyint(1)",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddForeignKey(
                name: "FK_Cases_Users_CaseCreatorId",
                table: "Cases",
                column: "CaseCreatorId",
                principalTable: "Users",
                principalColumn: "UserId",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
