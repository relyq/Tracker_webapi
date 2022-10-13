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

        [Required]
        public Guid OrganizationId { get; set; }

        public string? FirstName { get; set; }
        public string? LastName { get; set; }

        public DateTime? Created { get; set; }
    }
}
