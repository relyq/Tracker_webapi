using System.ComponentModel.DataAnnotations;

namespace Tracker.Models
{
    public class TicketStatus
    {
        public int Id { get; set; }

        [Required]
        [StringLength(50)]
        public string Status { get; set; }
    }
}
