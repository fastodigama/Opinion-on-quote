using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Opinion_on_Quotes.Models
{
    public class Quote
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int quote_id { get; set; } // Primary key, auto-generated

        // Each quote belongs to one drama (many-to-one relationship)
        [ForeignKey("drama_id")]
        public required virtual Drama? Drama { get; set; }

        public int drama_id { get; set; } // Foreign key referencing the drama

        public string? content { get; set; } // The actual quote text

        public string? actor { get; set; } // Actor who delivered the quote

        public int episode { get; set; } // Episode number where the quote appears

        // A quote can have many moods (many-to-many relationship)
        [System.Text.Json.Serialization.JsonIgnore]
        public ICollection<QuoteMood>? QuoteMoods { get; set; }

        // A quote can have multiple comments (one-to-many relationship)
        public ICollection<Comment>? Comments { get; set; }
    }

    public class QuoteDto
    {
        public int quote_id { get; set; } // Unique identifier for the quote

        public string? content { get; set; } // Quote text

        public string? actor { get; set; } // Actor name

        public int episode { get; set; } // Episode number

        public string drama_title { get; set; } // Title of the associated drama

        public int drama_id { get; set; } // Drama ID for reference

        public List<CommentDto>? comments { get; set; } // List of comments on the quote
    }
}
