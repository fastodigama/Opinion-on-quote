using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Opinion_on_Quotes.Models;

namespace Opinion_on_Quotes.Models
{
    public class Comment
    {
        [Key]
        public int CommentId { get; set; } // Primary key for the comment

        public string? CommentText { get; set; } // Text content of the comment

        public DateTime CreatedAt { get; set; } // Timestamp when the comment was created

        [ForeignKey("quote_id")]
        public required virtual Quote Quote { get; set; } // Navigation property to the related quote

        public int quote_id { get; set; } // Foreign key referencing the quote

        public string? UserId { get; set; } // ID of the user who posted the comment (from Identity)
    }

    public class CommentDto
    {
        public int CommentId { get; set; } // Unique identifier for the comment

        public string CommentText { get; set; } = "No comment provided."; // Default comment text if none is given

        public string CreatedAt { get; set; } = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"); // Formatted creation timestamp

        public string UserName { get; set; } // Display name of the user

        public string UserId { get; set; } // ID of the user (used for ownership checks)

        public int quote_id { get; set; } // ID of the quote this comment belongs to
    }

    public class CreateCommentDto
    {
        public string CommentText { get; set; } = "No comment provided."; // Default comment text for new comments

        public int quote_id { get; set; } // ID of the quote being commented on

        public DateTime CreatedAt { get; set; } // Timestamp when the comment is created
    }
}
