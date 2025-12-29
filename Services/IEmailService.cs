// Services/IEmailService.cs
namespace LabBenchManager.Services
{
    public interface IEmailService
    {
        Task SendEmailAsync(string toEmail, string subject, string htmlMessage);
    }
}