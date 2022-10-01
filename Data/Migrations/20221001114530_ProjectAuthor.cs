using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Tracker.Data.Migrations
{
    public partial class ProjectAuthor : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "AuthorId",
                table: "Project",
                type: "nvarchar(450)",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Project_AuthorId",
                table: "Project",
                column: "AuthorId");

            migrationBuilder.AddForeignKey(
                name: "FK_Project_AspNetUsers_AuthorId",
                table: "Project",
                column: "AuthorId",
                principalTable: "AspNetUsers",
                principalColumn: "Id");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Project_AspNetUsers_AuthorId",
                table: "Project");

            migrationBuilder.DropIndex(
                name: "IX_Project_AuthorId",
                table: "Project");

            migrationBuilder.DropColumn(
                name: "AuthorId",
                table: "Project");
        }
    }
}
