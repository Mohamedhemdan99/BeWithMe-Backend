using System.ComponentModel.DataAnnotations;

namespace BeWithMe.DTOs
{
    public class TokenVerificationDTO
    {
        [Required]
        [EmailAddress(ErrorMessage ="Email fromat Error")]
        [MaxLength(50)]
        public string Email { get; set; }

        [Required]
        public string Token { get; set; }
    }
}
