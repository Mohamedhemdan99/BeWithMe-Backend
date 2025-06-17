using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace BeWithMe.Models
{
    public class Patient //: ApplicationUser
    {
        public string? NeedsDescription { get; set; }
        public int HelpCount { get; set; } = 0;

        [Key, ForeignKey(nameof(User))]
        public string UserId { get; set; }
        public ApplicationUser User { get; set; }

    }

}
