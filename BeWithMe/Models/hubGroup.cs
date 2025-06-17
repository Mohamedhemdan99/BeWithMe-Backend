using System.ComponentModel.DataAnnotations;

namespace BeWithMe.Models
{
    public class hubGroup
    {
        [Key]
        public string GroupId { get; set; }      // Unique group ID (e.g., GUID)
        public string GroupName { get; set; }    // Human-readable group name

    }
}
