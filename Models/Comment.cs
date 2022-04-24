using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;
namespace Tracker.Models
{
    public class Comment
    {
        public int Id { get; set; }
        
        [Required]
        public int TicketId { get; set; }
        public Ticket Ticket { get; set; }

        [Required]
        public string AuthorId { get; set; }
        public ApplicationUser Author { get; set; }

        public int? ParentId { get; set; }
        public Comment? Parent { get; set; }

        public ICollection<Comment>? Replies { get; set; }

        [Required]
        public string Content { get; set; }

        [Required]
        [DataType(DataType.DateTime)]
        public DateTime Created { get; set; }
    }
}
