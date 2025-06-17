using System.ComponentModel.DataAnnotations;

namespace BeWithMe.DTOs
{
    public class VerifyCodeResponse
    {
        [MaxLength(255)]
        public string Token { get; set; }
    }
}
