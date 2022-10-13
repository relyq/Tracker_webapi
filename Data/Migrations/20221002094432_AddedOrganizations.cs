using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Tracker.Data.Migrations
{
    public partial class AddedOrganizations : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "OrganizationId",
                table: "TicketType",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "OrganizationId",
                table: "TicketStatus",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "OrganizationId",
                table: "Project",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "OrganizationId",
                table: "AspNetUsers",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "OrganizationId",
                table: "AspNetRoles",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "Organization",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false, defaultValueSql: "NEWID()"),
                    Name = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Organization", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_TicketType_OrganizationId",
                table: "TicketType",
                column: "OrganizationId");

            migrationBuilder.CreateIndex(
                name: "IX_TicketStatus_OrganizationId",
                table: "TicketStatus",
                column: "OrganizationId");

            migrationBuilder.CreateIndex(
                name: "IX_Project_OrganizationId",
                table: "Project",
                column: "OrganizationId");

            migrationBuilder.CreateIndex(
                name: "IX_AspNetUsers_OrganizationId",
                table: "AspNetUsers",
                column: "OrganizationId");

            migrationBuilder.CreateIndex(
                name: "IX_AspNetRoles_OrganizationId",
                table: "AspNetRoles",
                column: "OrganizationId");

            migrationBuilder.AddForeignKey(
                name: "FK_AspNetRoles_Organization_OrganizationId",
                table: "AspNetRoles",
                column: "OrganizationId",
                principalTable: "Organization",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_AspNetUsers_Organization_OrganizationId",
                table: "AspNetUsers",
                column: "OrganizationId",
                principalTable: "Organization",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Project_Organization_OrganizationId",
                table: "Project",
                column: "OrganizationId",
                principalTable: "Organization",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_TicketStatus_Organization_OrganizationId",
                table: "TicketStatus",
                column: "OrganizationId",
                principalTable: "Organization",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_TicketType_Organization_OrganizationId",
                table: "TicketType",
                column: "OrganizationId",
                principalTable: "Organization",
                principalColumn: "Id");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_AspNetRoles_Organization_OrganizationId",
                table: "AspNetRoles");

            migrationBuilder.DropForeignKey(
                name: "FK_AspNetUsers_Organization_OrganizationId",
                table: "AspNetUsers");

            migrationBuilder.DropForeignKey(
                name: "FK_Project_Organization_OrganizationId",
                table: "Project");

            migrationBuilder.DropForeignKey(
                name: "FK_TicketStatus_Organization_OrganizationId",
                table: "TicketStatus");

            migrationBuilder.DropForeignKey(
                name: "FK_TicketType_Organization_OrganizationId",
                table: "TicketType");

            migrationBuilder.DropTable(
                name: "Organization");

            migrationBuilder.DropIndex(
                name: "IX_TicketType_OrganizationId",
                table: "TicketType");

            migrationBuilder.DropIndex(
                name: "IX_TicketStatus_OrganizationId",
                table: "TicketStatus");

            migrationBuilder.DropIndex(
                name: "IX_Project_OrganizationId",
                table: "Project");

            migrationBuilder.DropIndex(
                name: "IX_AspNetUsers_OrganizationId",
                table: "AspNetUsers");

            migrationBuilder.DropIndex(
                name: "IX_AspNetRoles_OrganizationId",
                table: "AspNetRoles");

            migrationBuilder.DropColumn(
                name: "OrganizationId",
                table: "TicketType");

            migrationBuilder.DropColumn(
                name: "OrganizationId",
                table: "TicketStatus");

            migrationBuilder.DropColumn(
                name: "OrganizationId",
                table: "Project");

            migrationBuilder.DropColumn(
                name: "OrganizationId",
                table: "AspNetUsers");

            migrationBuilder.DropColumn(
                name: "OrganizationId",
                table: "AspNetRoles");
        }
    }
}
