using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Tracker.Data.Migrations
{
    public partial class UserRoleOrganization : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "PK_AspNetUserRoles",
                table: "AspNetUserRoles");

            migrationBuilder.AddColumn<Guid>(
                name: "OrganizationId",
                table: "AspNetUserRoles",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddPrimaryKey(
                name: "PK_AspNetUserRoles",
                table: "AspNetUserRoles",
                columns: new[] { "UserId", "RoleId", "OrganizationId" });

            migrationBuilder.CreateIndex(
                name: "IX_AspNetUserRoles_OrganizationId",
                table: "AspNetUserRoles",
                column: "OrganizationId");

            migrationBuilder.Sql("UPDATE AspNetUserRoles SET OrganizationId = ApplicationUserOrganization.OrganizationsId FROM AspNetUserRoles JOIN ApplicationUserOrganization ON AspNetUserRoles.UserId = ApplicationUserOrganization.UsersId;");

            migrationBuilder.AddForeignKey(
                name: "FK_AspNetUserRoles_Organization_OrganizationId",
                table: "AspNetUserRoles",
                column: "OrganizationId",
                principalTable: "Organization",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_AspNetUserRoles_Organization_OrganizationId",
                table: "AspNetUserRoles");

            migrationBuilder.DropPrimaryKey(
                name: "PK_AspNetUserRoles",
                table: "AspNetUserRoles");

            migrationBuilder.DropIndex(
                name: "IX_AspNetUserRoles_OrganizationId",
                table: "AspNetUserRoles");

            migrationBuilder.DropColumn(
                name: "OrganizationId",
                table: "AspNetUserRoles");

            migrationBuilder.AddPrimaryKey(
                name: "PK_AspNetUserRoles",
                table: "AspNetUserRoles",
                columns: new[] { "UserId", "RoleId" });
        }
    }
}
