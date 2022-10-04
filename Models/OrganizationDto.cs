using System.ComponentModel.DataAnnotations;

namespace Tracker.Models
{
    public class OrganizationDto
    {
        public Guid Id { get; set; }

        [Required]
        [StringLength(100)]
        public string Name { get; set; }

        public DateTime? Created { get; set; }
    }
}
