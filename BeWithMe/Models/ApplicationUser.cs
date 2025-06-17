using System.ComponentModel.DataAnnotations;
using BeWithMe.DTOs;
using Microsoft.AspNetCore.Identity;

namespace BeWithMe.Models
{
    public class ApplicationUser : IdentityUser
    {

        [Required]
        [StringLength(100)]
        [PersonalData]
        public string FullName { get; set; }

        [Required]
        [StringLength(10)]
        [PersonalData]
        public string Gender { get; set; }
        
        public bool Status { get; set; } = true;

        [PersonalData]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        [PersonalData]
        public DateTime? LastLogin { get; set; }


        [DataType(DataType.Date)]
        [PersonalData]
        public DateTime DateOfBirth { get; set; }
        public int Age => CalculateAge(DateOfBirth);

        public static int CalculateAge(DateTime birthDate)
        {
            DateTime today = DateTime.Today;
            int age = today.Year - birthDate.Year;

            // Check if birthday has occurred this year
            if (birthDate > today.AddYears(-age))
            {
                age--;
            }

            return age;
        }

        [PersonalData]
        public string ProfileImageUrl { get; set; }


        [StringLength(20)]
        [PersonalData]
        public string LanguagePreference { get; set; } = "arabic";

        public List<RefreshToken>? RefreshTokens { get; set; }

        public Patient? Patient { get; set; }
        public Helper? Helper { get; set; }

        public ICollection<Notification> Notifications { get; set; } = new List<Notification>();

        // Navigation property for sessions where the user is the patient
        //public ICollection<HelpSession> PatientSessions { get; set; } = new List<HelpSession>();

        // Navigation property for sessions where the user is the helper
        //public ICollection<HelpSession> HelperSessions { get; set; } = new List<HelpSession>();
        public ICollection<Post> AuthoredPosts { get; set; }
        public ICollection<PostReaction> AcceptedPosts { get; set; }

    }
 



}
