using BeWithMe.Models.Enums;

namespace BeWithMe.Models
{
    //public class CallHistory
    //{
    //    public int Id { get; set; }
    //    public int PostId { get; set; } // Optional: Link call to post
    //    public string CallerId { get; set; } // User who initiated the call
    //    public string CalleeId { get; set; } // User who received the call
    //    public DateTime StartTime { get; set; }
    //    public DateTime? EndTime { get; set; } = DateTime.UtcNow; // Null if ongoing
    //    public string RoomName { get; set; } // Twilio room name
    //    public callStatus Status { get; set; }

    //    // Navigation properties
    //    public Post Post { get; set; }
    //    public ApplicationUser Caller { get; set; }
    //    public ApplicationUser Callee { get; set; }
    //}
 
        public class CallHistory
        {
            public int Id { get; set; }
            public int PostId { get; set; } // Optional: Link call to post
            public string CallerId { get; set; } // User who initiated the call
            public string CalleeId { get; set; } // User who received the call
            public DateTime StartTime { get; set; }
            public DateTime? EndTime { get; set; } // Null if ongoing
            public string RoomName { get; set; } // Twilio room name
            public CallStatus Status { get; set; } = CallStatus.Initiated;
            public DateTime LastHeartbeat { get; set; } = DateTime.UtcNow;
            public string? DisconnectReason { get; set; } // Optional reason for disconnection

            // Navigation properties
            public Post Post { get; set; }
            public ApplicationUser Caller { get; set; }
            public ApplicationUser Callee { get; set; }
        }
    
}
