namespace BeWithMe.DTOs
{
    public class CallHistoryDto
    {
        public int Id { get; set; }
        public int PostId { get; set; }
        public string RoomName { get; set; }
        public UserDto Caller { get; set; }
        public UserDto Callee { get; set; }
        public DateTime StartTime { get; set; }
        public DateTime? EndTime { get; set; }
        public string Status { get; set; }
        public double? Duration { get; set; }
    }
}
