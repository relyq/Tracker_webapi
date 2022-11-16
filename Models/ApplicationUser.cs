using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.AspNetCore.Identity;

namespace Tracker.Models
{
    public class ApplicationUser : IdentityUser
    {
        [Required]
        public ICollection<Organization> Organizations { get; set; }

        [NotMapped]
        public ICollection<UserRole> Roles { get; set; }

        [StringLength(50)]
        public string? FirstName { get; set; }

        [StringLength(50)]
        public string? LastName { get; set; }

        public ICollection<Comment>? Comments { get; set; }

        [Required]
        [DataType(DataType.DateTime)]
        public DateTime Created { get; set; }

        [DataType(DataType.DateTime)]
        public DateTime? Updated { get; set; }
    }
}
