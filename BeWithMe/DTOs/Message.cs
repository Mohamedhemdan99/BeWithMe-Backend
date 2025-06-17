using MimeKit;

namespace BeWithMe.DTOs
{
    public class Message
    {
        public List<MailboxAddress> To { get; set; }
        public string Subject { get; set; }
        public string Body { get; set; }
        public string? Token { get; set; } = string.Empty;

        public Message(IEnumerable<string> to, string subject, string body, string? token = null)
        {
            To = new List<MailboxAddress>();
            To.AddRange(to.Select(x => new MailboxAddress("email", x)));
            Subject = subject;
            Body = body;
            Token = token;
        }
    }
}
