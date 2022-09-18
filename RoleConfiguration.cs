using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Tracker
{
    public class RoleConfiguration : IEntityTypeConfiguration<IdentityRole>
    {
        public void Configure(EntityTypeBuilder<IdentityRole> builder)
        {
            builder.HasData(
                new IdentityRole
                {
                    Id = "132d3801-ad39-4778-92fc-14dee8b8d2a8",
                    ConcurrencyStamp = "cda63a26-c133-4181-9e00-4b4b7b6d9049",
                    Name = "Administrator",
                    NormalizedName = "Administrator".ToUpper()
                },
                new IdentityRole
                {
                    Id = "e5b62a8c-80dd-40e2-9fd7-7d293cb23ff0",
                    ConcurrencyStamp = "503ea131-c626-49f1-a9fc-0c074742d51b",
                    Name = "Developer",
                    NormalizedName = "Developer".ToUpper()
                },
                new IdentityRole
                {
                    Id = "3d865d1e-3dea-4a58-ae93-0b2eaa78ac79",
                    ConcurrencyStamp = "2b4a5ef8-ed84-4ac6-b638-35235540d477",
                    Name = "User",
                    NormalizedName = "User".ToUpper()
                }
                );
        }
    }
}
