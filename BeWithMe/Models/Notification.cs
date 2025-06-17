using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using BeWithMe.Models.Enums;

namespace BeWithMe.Models
{
    public class Notification
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public string UserId { get; set; }

        public string RecipientId { get; set; }    // Recipient ID

        [Required]
        [StringLength(500)]
        public string Content { get; set; }

        public bool IsRead { get; set; } = false;
        //public string profilePictureUrl { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        [StringLength(50)]
        public string Type { get; set; } = NotificationType.General; // General, Help Request, System, etc.

        // Foreign key relationship
        [ForeignKey("UserId")]
        public ApplicationUser User { get; set; }
    }
}
