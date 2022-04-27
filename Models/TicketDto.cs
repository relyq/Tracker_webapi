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
        public string Type { get; set; }

        [Required]
        public int TicketStatusId { get; set; }
        public string Status { get; set; }

        [Required]
        public string SubmitterId { get; set; }
        [Display(Name = "Submitter")]
        public string SubmitterUsername { get; set; }

        [Required]
        public string AssigneeId { get; set; }
        [Display(Name = "Assignee")]
        public string AssigneeUsername { get; set; }

        public ICollection<CommentDto>? Comments { get; set; }

        [Required]
        [DataType(DataType.DateTime)]
        public DateTime Created { get; set; }

        [DataType(DataType.DateTime)]
        public DateTime? Closed { get; set; }
    }
}
