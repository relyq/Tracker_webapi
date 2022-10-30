using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Tracker.Data.Migrations
{
    public partial class NormalizedType : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "NormalizedType",
                table: "TicketType",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: false,
                defaultValue: "");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "NormalizedType",
                table: "TicketType");
        }
    }
}
