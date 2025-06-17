using System.ComponentModel.DataAnnotations;

namespace BeWithMe.DTOs
{
    public class ResetPasswordDTO
    {
        [EmailAddress]
        [Required(ErrorMessage = "The Email is Required")]
        [MaxLength(200)]
        public string Email  { get; set; }

        [DataType(DataType.Password)]
        public string Password { get; set; }

        [DataType(DataType.Password)]
        //[Compare(nameof(Password))]
        public string ConfirmPassword { get; set; }
        [Required]
        [MaxLength(255)]
        public string Token {  get; set; }
    }
}
