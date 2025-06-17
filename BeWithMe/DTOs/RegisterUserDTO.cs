using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using BeWithMe.Models;

namespace BeWithMe.DTOs
{
    public class RegisterUserDTO
    {
        [EmailAddress]
        [Required(ErrorMessage ="The Email is Required")]
        [MaxLength(200)]
        public string Email { get; set; }

        [Required(ErrorMessage = "The Password is Required")]
        [MaxLength(50)]
        [DataType(DataType.Password)]
        public string Password { get; set; }

        [DataType(DataType.Password)]
        [Compare("Password")]
        public string ConfirmPassword   { get; set; }

        //[Required(ErrorMessage = "The UserName is Required")]
        [MaxLength(20)]
        //[Unique]   //Check if the userName doesn't  Exist in the DataBase
        public string? Username { get; set; } 

        public string Role { get; set; }
        public string Gender { get; set; }
        public string FullName { get; set; }
        public DateTime DateOfBirth { get; set; }

        // image 
        public IFormFile? ProfileImage { get; set; }


    }

 
}
