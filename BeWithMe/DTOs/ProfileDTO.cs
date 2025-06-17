using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;
using BeWithMe.Services;

namespace BeWithMe.DTOs
{
    public class ProfileDTO
    {
        public string? FullName { get; set; }
        public string? Gender { get; set; }

        [JsonConverter(typeof(ConvertDateFormat))]
        public DateTime? DateOfBirth { get; set; }
        //public string? Email { get; set; }
        public string? LanguagePreference { get; set; }
        //public string? NeedsDescription { get; set; }
        public IFormFile? ProfileImage { get; set; }

        public string? Password { get; set; }

        [Compare("Password", ErrorMessage = "The password and confirmation password do not match.")]
        public string? ConfirmPassword { get; set; }

    }
}
