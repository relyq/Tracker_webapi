using System.ComponentModel.DataAnnotations;

namespace Tracker.Models
{
    public class ProjectDto
    {
        [Required]
        public int Id { get; set; }

        [Required]
        [StringLength(100)]
        public string Name { get; set; }

        [StringLength(500)]
        public string? Description { get; set; }
        
        [Required]
        [DataType(DataType.DateTime)]
        public DateTime Created { get; set; }
    }
}
