using System.ComponentModel.DataAnnotations;

namespace BeWithMe.Models
{
    public class hubUserConnections
    {

        public string UserId { get; set; }      // User identifier
        [Key]
        public string ConnectionId { get; set; } // SignalR connection ID

    }
}
