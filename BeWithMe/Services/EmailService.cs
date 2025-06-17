using BeWithMe.DTOs;
using BeWithMe.Repository.Interfaces;
using MailKit.Net.Smtp;
using MailKit.Security;
using Microsoft.Extensions.Options;
using MimeKit;


namespace BeWithMe.Services
{
    public class EmailService : IEmailService
    {
        private readonly EmailConfiguration _emailConfiguration;
        private readonly ILogger<EmailService> _logger;

        
        public EmailService(ILogger<EmailService> logger, IOptions<EmailConfiguration> emailConfig)
        {
            _logger = logger;
            _emailConfiguration = emailConfig.Value;
        }

        public void SendEmail(Message message)
        {
            var MessageMail = CreateMessage(message);
            _logger.LogInformation("Message Created Successfully");
            Send(MessageMail);
        }

        private MimeMessage CreateMessage(Message message)
        {
            var emailMessage = new MimeMessage();

            emailMessage.From.Add(new MailboxAddress("BeWithMe", _emailConfiguration.From));
            emailMessage.To.AddRange(message.To);
            emailMessage.Subject = message.Subject;
            emailMessage.Body = new TextPart(MimeKit.Text.TextFormat.Html) { Text = message.Body };
           

            //var bodyBuilder = new BodyBuilder();
            //bodyBuilder.HtmlBody = $"<p>Hello,</p>" +
            //                       $"<p>Click <a href=\"{message.Token}\">here</a> to reset your password.</p>" +
            //                       $"<p>If you didn’t request this, please ignore this email.</p>";

            //emailMessage.Body = bodyBuilder.ToMessageBody();

            return emailMessage;
        }

        private void Send(MimeMessage message)
        {
            using var smtp = new SmtpClient();
            try
            {
                smtp.Connect(_emailConfiguration.SmtpServer, _emailConfiguration.Port,SecureSocketOptions.StartTls);
                _logger.LogInformation("Connection Established Successfully");
                smtp.AuthenticationMechanisms.Remove("XOAUTH2");
                smtp.Authenticate(_emailConfiguration.UserName, _emailConfiguration.Password);
                smtp.Send(message);
                _logger.LogInformation("Message Sent");
            }
            catch (Exception ex)
            {
                _logger.LogError("Failed To Send Email Message");
                throw;
            }
            finally
            {
                smtp.Disconnect(true);
                smtp.Dispose();
                _logger.LogInformation("Connection Diconnected and Disposed");
            }
            
        }
    }
}
