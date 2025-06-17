namespace BeWithMe.Repository.Interfaces
{
    public interface IEmailService
    {
        public void SendEmail(DTOs.Message message);
    }
}
