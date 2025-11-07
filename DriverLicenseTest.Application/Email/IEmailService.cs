namespace Application.Email
{
    public interface IEmailService
    {
        Task SendConfirmationEmailAsync(string toEmail, string subject, string content);
    }
}
