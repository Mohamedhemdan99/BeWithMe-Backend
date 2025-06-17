using System.ComponentModel.DataAnnotations.Schema;
using BeWithMe.Models.Enums;

namespace BeWithMe.Models
{
    public class PostReaction
    {
        public int Id { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        [ForeignKey("PostId")]
        public int PostId { get; set; }
        public Post Post { get; set; }
        public string AcceptorId { get; set; }
        [ForeignKey("AcceptorId")]
        public Helper Helper { get; set; }
    }
}
