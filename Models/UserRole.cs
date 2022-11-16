using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Tracker.Models
{
    public class UserRole : IdentityUserRole<string>
    {
        public Guid OrganizationId { get; set; }
        public Organization Organization { get; set; }
    }
}
