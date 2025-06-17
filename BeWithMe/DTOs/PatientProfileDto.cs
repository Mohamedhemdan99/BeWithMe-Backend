using BeWithMe.Services;
using System.Text.Json.Serialization;

namespace BeWithMe.DTOs
{
    public class PatientProfileDto 
    {
        public string userId { get; set; }
        public string FullName { get; set; }
        public string userName { get; set; } // Add this lin
        public string Gender { get; set; }

        [JsonConverter(typeof(ConvertDateFormat))]
        public DateTime? DateOfBirth { get; set; }
        public string Email { get; set; }
        public string LanguagePreference { get; set; }
        //public string NeedsDescription { get; set; }
        public int HelpCount { get; set; }
        public string ProfileImageUrl { get; set; } 
    }
}
