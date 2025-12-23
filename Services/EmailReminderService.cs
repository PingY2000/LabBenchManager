using LabBenchManager.Data;
using LabBenchManager.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Net;
using System.Net.Mail;
using System.Threading.Tasks;

namespace LabBenchManager.Services
{
    public class EmailReminderService
    {
        private readonly LabDbContext _context;
        private readonly IConfiguration _configuration;

        public EmailReminderService(LabDbContext context, IConfiguration configuration)
        {
            _context = context;
            _configuration = configuration;
        }

        public async Task CheckAndSendRemindersAsync()
        {
            // 开发环境：改为1分钟超时，方便测试
            var threshold = _configuration.GetValue<bool>("IsDevelopment")
                ? DateTime.UtcNow.AddMinutes(-1) // 开发环境：1分钟
                : DateTime.UtcNow.AddDays(-10);  // 生产环境：10天

            Console.WriteLine($"[邮件提醒] 检查时间阈值: {threshold:yyyy-MM-dd HH:mm:ss}");

            // 查找超过阈值未审核的报告
            var overdueReviews = await _context.ReportApprovals
                .Where(r => r.Status == ReportApprovalStatus.待审核 && r.SubmitTime < threshold)
                .ToListAsync();

            Console.WriteLine($"[邮件提醒] 找到 {overdueReviews.Count} 个超时待审核报告");

            foreach (var report in overdueReviews)
            {
                var reviewer = await _context.ApplicationUsers
                    .FirstOrDefaultAsync(u => u.NtAccount == report.ReviewerNTAccount);

                if (reviewer != null && !string.IsNullOrEmpty(reviewer.Email))
                {
                    await SendReminderEmailAsync(
                        reviewer.Email,
                        reviewer.DisplayName ?? reviewer.NtAccount,
                        report.ReportTitle,
                        "审核",
                        report.Id
                    );
                }
                else
                {
                    Console.WriteLine($"[警告] 审核人 {report.ReviewerNTAccount} 未设置邮箱，跳过发送");
                }
            }

            // 查找超过阈值未批准的报告
            var overdueApprovals = await _context.ReportApprovals
                .Where(r => r.Status == ReportApprovalStatus.待批准 &&
                           r.ReviewTime.HasValue && r.ReviewTime.Value < threshold)
                .ToListAsync();

            Console.WriteLine($"[邮件提醒] 找到 {overdueApprovals.Count} 个超时待批准报告");

            foreach (var report in overdueApprovals)
            {
                var approver = await _context.ApplicationUsers
                    .FirstOrDefaultAsync(u => u.NtAccount == report.ApproverNTAccount);

                if (approver != null && !string.IsNullOrEmpty(approver.Email))
                {
                    await SendReminderEmailAsync(
                        approver.Email,
                        approver.DisplayName ?? approver.NtAccount,
                        report.ReportTitle,
                        "批准",
                        report.Id
                    );
                }
                else
                {
                    Console.WriteLine($"[警告] 批准人 {report.ApproverNTAccount} 未设置邮箱，跳过发送");
                }
            }
        }

        private async Task SendReminderEmailAsync(string toEmail, string toName, string reportTitle, string action, int reportId)
        {
            // 开发环境：只输出到控制台，不真正发送
            if (_configuration.GetValue<bool>("IsDevelopment"))
            {
                Console.WriteLine("=".PadRight(80, '='));
                Console.WriteLine($"📧 [模拟邮件发送]");
                Console.WriteLine($"收件人: {toName} <{toEmail}>");
                Console.WriteLine($"主题: 提醒：待{action}报告 - {reportTitle}");
                Console.WriteLine($"内容: 您有一份报告《{reportTitle}》已超过规定时间未{action}，请尽快处理。");
                Console.WriteLine($"报告ID: {reportId}");
                Console.WriteLine($"链接: /report-approvals");
                Console.WriteLine($"时间: {DateTime.Now:yyyy-MM-dd HH:mm:ss}");
                Console.WriteLine("=".PadRight(80, '='));
                return;
            }

            // 生产环境：真正发送邮件
            try
            {
                var smtpServer = _configuration["Email:SmtpServer"] ?? "smtp.bosch.com";
                var smtpPort = int.Parse(_configuration["Email:SmtpPort"] ?? "25");
                var fromEmail = _configuration["Email:FromAddress"] ?? "noreply@bosch.com";

                var message = new MailMessage
                {
                    From = new MailAddress(fromEmail, "实验室管理系统"),
                    Subject = $"提醒：待{action}报告 - {reportTitle}",
                    Body = $@"
                        <html>
                        <body>
                            <p>尊敬的 {toName}，</p>
                            <p>您有一份报告《<strong>{reportTitle}</strong>》已超过10天未{action}，请尽快处理。</p>
                            <p><a href='{_configuration["AppUrl"]}/report-approvals' style='background-color:#0d6efd;color:white;padding:10px 20px;text-decoration:none;border-radius:5px;'>点击查看报告</a></p>
                            <hr/>
                            <p style='color:#666;font-size:12px;'>此邮件由系统自动发送，请勿回复。</p>
                        </body>
                        </html>
                    ",
                    IsBodyHtml = true
                };

                message.To.Add(toEmail);

                using var smtp = new SmtpClient(smtpServer, smtpPort)
                {
                    EnableSsl = false,
                    Credentials = CredentialCache.DefaultNetworkCredentials
                };

                await smtp.SendMailAsync(message);
                Console.WriteLine($"[邮件提醒] 已发送邮件给 {toEmail}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[错误] 发送邮件失败: {ex.Message}");
            }
        }
    }
}