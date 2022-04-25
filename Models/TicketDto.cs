using System.ComponentModel.DataAnnotations;

namespace Tracker.Models
{
    public class TicketDto
    {
        public int Id { get; set; }

        [Required]
        public int ProjectId { get; set; }

        [Required]
        [StringLength(100)]
        public string Title { get; set; }

        [Required]
        public string Description { get; set; }

        [Required]
        [Range(1, 5)]
        public int Priority { get; set; }

        [Required]
        public int TicketTypeId { get; set; }

        [Required]
        public int TicketStatusId { get; set; }

        [Required]
        public string SubmitterId { get; set; }

        [Required]
        public string AssigneeId { get; set; }

        [Required]
        [DataType(DataType.DateTime)]
        public DateTime Created { get; set; }

        [DataType(DataType.DateTime)]
        public DateTime? Closed { get; set; }
    }
}
