using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace Tracker.Models
{
    public class Comment
    {
        public int Id { get; set; }
        
        [Required]
        [ForeignKey("Ticket")]
        public int TicketId { get; set; }
        [JsonIgnore]
        public Ticket Ticket { get; set; }

        [Required]
        public string AuthorId { get; set; }
        [JsonIgnore]
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
