using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Tracker.Data.Migrations
{
    public partial class TicketCommentOnDeleteCascade : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Comment_Ticket_TicketId",
                table: "Comment");

            migrationBuilder.AddForeignKey(
                name: "FK_Comment_Ticket_TicketId",
                table: "Comment",
                column: "TicketId",
                principalTable: "Ticket",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Comment_Ticket_TicketId",
                table: "Comment");

            migrationBuilder.AddForeignKey(
                name: "FK_Comment_Ticket_TicketId",
                table: "Comment",
                column: "TicketId",
                principalTable: "Ticket",
                principalColumn: "Id");
        }
    }
}
