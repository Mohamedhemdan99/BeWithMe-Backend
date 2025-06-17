using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using BeWithMe.Models.Enums;

namespace BeWithMe.Models
{
    public class Post
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public string UserId { get; set; }

        [Required]
        [StringLength(1000)]
        public string Content { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // Add Status property
        public PostStatus Status { get; set; } = PostStatus.Pending;

        // Foreign key relationship
        [ForeignKey("UserId")]
        public ApplicationUser User { get; set; }

        // Navigation property for the accepted help session (nullable, as a post might not be accepted yet)
        //public HelpSession? HelpSession { get; set; }
        public ICollection<PostReaction> Reactions { get; set; }
    }
} 