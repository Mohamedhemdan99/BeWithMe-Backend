using System.ComponentModel.DataAnnotations;

namespace BeWithMe.Models
{
    public class PasswordResetToken
    {
        public int Id { get; set; }
        [MaxLength(255)]
        public string UserId { get; set; }  // Links to the user
        [MaxLength(255)]
        public string Token { get; set; }   // The reset token
        [MaxLength(6)]
        public string Code { get; set; }
        public string Email { get; set; }  // Links to the user

        public DateTime ExpirationTime { get; set; }  // Expiration timestamp
    }
}
