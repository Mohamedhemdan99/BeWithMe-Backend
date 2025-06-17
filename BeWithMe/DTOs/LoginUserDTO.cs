using System.ComponentModel.DataAnnotations;

namespace BeWithMe.DTOs
{
    public class LoginUserDTO
    {

        [Required(ErrorMessage = "The Password is Required")]
        [MaxLength(50)]
        [MinLength(8)]
        [DataType(DataType.Password)]
        public string Password { get; set; }

        public string UsernameOrEmail { get; set; }

        public bool RememberMe { get; set; }
    }
}
