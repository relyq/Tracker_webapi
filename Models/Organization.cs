using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;

namespace Tracker.Models
{
    public class Organization
    {
        public Guid Id { get; set; }

        [Required]
        [StringLength(100)]
        public string Name { get; set; }

        public DateTime Created { get; set; }

        public IList<ApplicationUser>? Users { get; set; }

        public IList<Project>? Projects { get; set; }

        public IList<IdentityRole>? Roles { get; set; }

        public IList<TicketType>? TicketTypes { get; set; }

        public IList<TicketStatus>? TicketStatuses { get; set; }
    }
}
