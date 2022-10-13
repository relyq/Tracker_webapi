using System.ComponentModel.DataAnnotations;

namespace Tracker.Models
{
    public class TicketType
    {
        public int Id { get; set; }
        
        [Required]
        [StringLength(50)]
        public string Type { get; set; }

        [Required]
        public Guid OrganizationId { get; set; }
        public Organization Organization { get; set; }

        public override string ToString()
        {
            return Type;
        }
    }
}
