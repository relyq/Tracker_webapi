using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Tracker.Data.Migrations
{
    public partial class RolesDeveloperUser : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.InsertData(
                table: "AspNetRoles",
                columns: new[] { "Id", "ConcurrencyStamp", "Name", "NormalizedName" },
                values: new object[] { "3d865d1e-3dea-4a58-ae93-0b2eaa78ac79", "2b4a5ef8-ed84-4ac6-b638-35235540d477", "User", "USER" });

            migrationBuilder.InsertData(
                table: "AspNetRoles",
                columns: new[] { "Id", "ConcurrencyStamp", "Name", "NormalizedName" },
                values: new object[] { "e5b62a8c-80dd-40e2-9fd7-7d293cb23ff0", "503ea131-c626-49f1-a9fc-0c074742d51b", "Developer", "DEVELOPER" });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: "3d865d1e-3dea-4a58-ae93-0b2eaa78ac79");

            migrationBuilder.DeleteData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: "e5b62a8c-80dd-40e2-9fd7-7d293cb23ff0");
        }
    }
}
