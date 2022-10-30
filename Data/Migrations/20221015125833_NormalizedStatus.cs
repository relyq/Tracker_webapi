using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Tracker.Data.Migrations
{
    public partial class NormalizedStatus : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "NormalizedStatus",
                table: "TicketStatus",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: false,
                defaultValue: "");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "NormalizedStatus",
                table: "TicketStatus");
        }
    }
}
