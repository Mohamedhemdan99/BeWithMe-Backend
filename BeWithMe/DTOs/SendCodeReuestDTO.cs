using System.ComponentModel.DataAnnotations;

namespace BeWithMe.DTOs
{
    public class SendCodeReuestDTO
    {
        
        [EmailAddress]
        [Required(ErrorMessage = "The Email is Required")]
        [MaxLength(200)]
        public string Email { get; set; }
    }
}
