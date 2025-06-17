namespace BeWithMe.Repository
{
    public class AuthResponse
    {
        public string UserId { get; set; }
        public string Email { get; set; }
        public string Token { get; set; }
        public int ExpiresIn { get; set; }
    }
}
