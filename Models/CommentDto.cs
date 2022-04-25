﻿using System.ComponentModel.DataAnnotations;

namespace Tracker.Models
{
    public class CommentDto
    {
        public int Id { get; set; }

        [Required]
        public int TicketId { get; set; }

        [Required]
        public string AuthorId { get; set; }

        public int? ParentId { get; set; }

        [Required]
        public string Content { get; set; }

        [Required]
        [DataType(DataType.DateTime)]
        public DateTime Created { get; set; }
    }
}
