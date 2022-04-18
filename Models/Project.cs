using System.ComponentModel.DataAnnotations;

namespace Tracker.Models
{
    public class Project
    {
        public int Id { get; set; }
        [Required]
        [StringLength(100)]
        public string Name { get; set; }

        [StringLength(500)]
        public string? Description { get; set; }

        public ICollection<Ticket>? Tickets { get; set; }
        
        [DataType(DataType.DateTime)]
        public DateTime Created { get; set; }
    }
}
