// Services/EmailService.cs
using LabBenchManager.Models;
using MailKit.Net.Smtp;
using MailKit.Security;
using Microsoft.Extensions.Options;
using MimeKit;
using MimeKit.Text;

namespace LabBenchManager.Services
{
    public class EmailService : IEmailService
    {
        private readonly EmailSettings _emailSettings;
        private readonly ILogger<EmailService> _logger;

        public EmailService(IOptions<EmailSettings> emailSettings, ILogger<EmailService> logger)
        {
            _emailSettings = emailSettings.Value;
            _logger = logger;
        }

        public async Task SendEmailAsync(string toEmail, string subject, string htmlMessage)
        {
            try
            {
                var email = new MimeMessage();
                email.From.Add(new MailboxAddress(_emailSettings.SenderName, _emailSettings.SenderEmail));
                email.To.Add(MailboxAddress.Parse(toEmail));
                email.Subject = subject;
                email.Body = new TextPart(TextFormat.Html) { Text = htmlMessage };

                using var smtp = new SmtpClient();
                // 连接到服务器
                await smtp.ConnectAsync(_emailSettings.SmtpServer, _emailSettings.Port, SecureSocketOptions.StartTls);

                // 验证身份
                await smtp.AuthenticateAsync(_emailSettings.UserName, _emailSettings.Password);

                // 发送邮件
                await smtp.SendAsync(email);

                // 断开连接
                await smtp.DisconnectAsync(true);

                _logger.LogInformation($"Email sent successfully to {toEmail}");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to send email to {ToEmail}", toEmail);
                // 在实际应用中，你可能希望重新抛出异常或返回一个结果对象
                throw;
            }
        }
    }
}