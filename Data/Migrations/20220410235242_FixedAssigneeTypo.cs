using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Tracker.Data.Migrations
{
    public partial class FixedAssigneeTypo : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Ticket_AspNetUsers_AsigneeId",
                table: "Ticket");

            migrationBuilder.RenameColumn(
                name: "AsigneeId",
                table: "Ticket",
                newName: "AssigneeId");

            migrationBuilder.RenameIndex(
                name: "IX_Ticket_AsigneeId",
                table: "Ticket",
                newName: "IX_Ticket_AssigneeId");

            migrationBuilder.AddForeignKey(
                name: "FK_Ticket_AspNetUsers_AssigneeId",
                table: "Ticket",
                column: "AssigneeId",
                principalTable: "AspNetUsers",
                principalColumn: "Id");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Ticket_AspNetUsers_AssigneeId",
                table: "Ticket");

            migrationBuilder.RenameColumn(
                name: "AssigneeId",
                table: "Ticket",
                newName: "AsigneeId");

            migrationBuilder.RenameIndex(
                name: "IX_Ticket_AssigneeId",
                table: "Ticket",
                newName: "IX_Ticket_AsigneeId");

            migrationBuilder.AddForeignKey(
                name: "FK_Ticket_AspNetUsers_AsigneeId",
                table: "Ticket",
                column: "AsigneeId",
                principalTable: "AspNetUsers",
                principalColumn: "Id");
        }
    }
}
