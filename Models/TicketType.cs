using System.ComponentModel.DataAnnotations;

namespace Tracker.Models
{
    public class TicketType
    {
        public int Id { get; set; }
        
        [Required]
        [StringLength(50)]
        public string Type { get; set; }

        public override string ToString()
        {
            return Type;
        }
    }
}
