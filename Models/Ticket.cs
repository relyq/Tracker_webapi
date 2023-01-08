using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.ComponentModel.DataAnnotations;
using System.Runtime.InteropServices;
using System.Text.Json.Serialization;

namespace Tracker.Models
{
    public class Ticket
    {
        public int Id { get; set; }

        [Required]
        public int ProjectId { get; set; }
        public Project Project { get; set; }

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
        public TicketType Type { get; set; }

        [Required]
        public int TicketStatusId { get; set; }
        public TicketStatus Status { get; set; }

        [Display(Name = "Submitter")]
        [Required]
        public string SubmitterId { get; set; }
        public ApplicationUser Submitter { get; set; }

        [Display(Name = "Assignee")]
        [Required]
        public string AssigneeId { get; set; }
        public ApplicationUser Assignee { get; set; }

        public ICollection<Comment>? Comments { get; set; }

        [Required]
        [DataType(DataType.DateTime)]
        public DateTime Created { get; set; }
        
        [DataType(DataType.DateTime)]
        public DateTime? Activity { get; set; }

        [DataType(DataType.DateTime)]
        public DateTime? Closed { get; set; }
    }
}