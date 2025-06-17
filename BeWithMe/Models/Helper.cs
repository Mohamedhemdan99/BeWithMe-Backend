using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace BeWithMe.Models
{
    public class Helper //: ApplicationUser
    {
        [Key, ForeignKey(nameof(User))]
        public string UserId { get; set; }
        public ApplicationUser User { get; set; }
        public decimal Rate { get; set; } = 0;
        //public ICollection<HelpInstance> ProvidedHelp { get; set; } = new List<HelpInstance>();
    }
}
