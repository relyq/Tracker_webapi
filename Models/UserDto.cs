using System.ComponentModel.DataAnnotations;

namespace Tracker.Models
{
    public class UserDto
    {
        [Required]
        public string Id { get; set; }

        [Required]
        public string Username { get; set; }

        [Required]
        public string Email { get; set; }

        public ICollection<Guid>? OrganizationsId { get; set; }

        public ICollection<OrganizationRole>? Roles { get; set; }

        public string? FirstName { get; set; }
        public string? LastName { get; set; }

        public string? PhoneNumber { get; set; }

        public DateTime? Created { get; set; }
    }

    public class OrganizationRole
    {
        public Guid OrganizationId { get; set; }
        public string RoleId { get; set; }

        public OrganizationRole(Guid organizationId, string roleId)
        {
            OrganizationId = organizationId;
            RoleId = roleId;
        }
    }
}
