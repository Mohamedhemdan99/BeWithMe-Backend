namespace BeWithMe.Models.Enums
{
    public enum PostStatus
    {
        Pending,    // Initial state, waiting for a helper
        Accepted,   // A helper has accepted the request
        Completed,  // The help session is marked as completed
        Cancelled   // The post/request was cancelled
    }
} 