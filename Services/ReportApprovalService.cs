// Services/ReportApprovalService.cs
using DocumentFormat.OpenXml.InkML;
using LabBenchManager.Data;
using LabBenchManager.Models;
using Microsoft.AspNetCore.Components.Forms;
using Microsoft.EntityFrameworkCore;

namespace LabBenchManager.Services
{
    public class ReportApprovalService
    {
        private readonly IDbContextFactory<LabDbContext> _contextFactory;
        private readonly IWebHostEnvironment _webHostEnvironment;

        public ReportApprovalService(
            IDbContextFactory<LabDbContext> contextFactory,
            IWebHostEnvironment webHostEnvironment)
        {
            _contextFactory = contextFactory;
            _webHostEnvironment = webHostEnvironment;
        }

        // 获取所有报告
        public async Task<List<ReportApproval>> GetAllReportsAsync()
        {
            await using var db = await _contextFactory.CreateDbContextAsync();
            return await db.ReportApprovals
                .Include(r => r.Assignment)
                .OrderByDescending(r => r.SubmitTime)
                .ToListAsync();
        }

        // 根据ID获取报告
        public async Task<ReportApproval?> GetReportByIdAsync(int id)
        {
            await using var db = await _contextFactory.CreateDbContextAsync();
            return await db.ReportApprovals
                .Include(r => r.Assignment)
                .FirstOrDefaultAsync(r => r.Id == id);
        }

        // 获取我提交的报告
        public async Task<List<ReportApproval>> GetMySubmittedReportsAsync(string ntAccount)
        {
            await using var db = await _contextFactory.CreateDbContextAsync();
            return await db.ReportApprovals
                .Include(r => r.Assignment)
                .Where(r => r.SubmitterNTAccount == ntAccount)
                .OrderByDescending(r => r.SubmitTime)
                .ToListAsync();
        }

        // 获取待我审核的报告
        public async Task<List<ReportApproval>> GetMyReviewTasksAsync(string ntAccount)
        {
            await using var db = await _contextFactory.CreateDbContextAsync();
            return await db.ReportApprovals
                .Include(r => r.Assignment)
                .Where(r => r.ReviewerNTAccount == ntAccount && r.Status == ReportApprovalStatus.待审核)
                .OrderBy(r => r.SubmitTime)
                .ToListAsync();
        }

        // 获取待我批准的报告
        public async Task<List<ReportApproval>> GetMyApprovalTasksAsync(string ntAccount)
        {
            await using var db = await _contextFactory.CreateDbContextAsync();
            return await db.ReportApprovals
                .Include(r => r.Assignment)
                .Where(r => r.ApproverNTAccount == ntAccount && r.Status == ReportApprovalStatus.待批准)
                .OrderBy(r => r.ReviewTime)
                .ToListAsync();
        }

        // 创建报告
        public async Task<ReportApproval> CreateReportAsync(ReportApproval report)
        {
            await using var db = await _contextFactory.CreateDbContextAsync();
            report.SubmitTime = DateTime.UtcNow;
            db.ReportApprovals.Add(report);
            await db.SaveChangesAsync();
            return report;
        }

        // 更新报告
        public async Task<ReportApproval> UpdateReportAsync(ReportApproval report)
        {
            await using var db = await _contextFactory.CreateDbContextAsync();
            db.ReportApprovals.Update(report);
            await db.SaveChangesAsync();
            return report;
        }

        // 提交报告（从草稿变为待审核）
        public async Task<ReportApproval> SubmitReportAsync(int reportId)
        {
            await using var db = await _contextFactory.CreateDbContextAsync();
            var report = await db.ReportApprovals.FindAsync(reportId);
            if (report != null && report.Status == ReportApprovalStatus.草稿)
            {
                report.Status = ReportApprovalStatus.待审核;
                report.SubmitTime = DateTime.UtcNow;
                await db.SaveChangesAsync();
            }
            return report!;
        }

        // 审核通过
        public async Task<ReportApproval> ApproveReviewAsync(int reportId, string comments)
        {
            await using var db = await _contextFactory.CreateDbContextAsync();
            var report = await db.ReportApprovals.FindAsync(reportId);
            if (report != null && report.Status == ReportApprovalStatus.待审核)
            {
                report.Status = ReportApprovalStatus.待批准;
                report.ReviewTime = DateTime.UtcNow;
                report.ReviewComments = comments;
                await db.SaveChangesAsync();
            }
            return report!;
        }

        // 审核驳回
        public async Task<ReportApproval> RejectReviewAsync(int reportId, string comments)
        {
            await using var db = await _contextFactory.CreateDbContextAsync();
            var report = await db.ReportApprovals.FindAsync(reportId);
            if (report != null && report.Status == ReportApprovalStatus.待审核)
            {
                report.Status = ReportApprovalStatus.审核驳回;
                report.ReviewTime = DateTime.UtcNow;
                report.ReviewComments = comments;
                await db.SaveChangesAsync();
            }
            return report!;
        }

        // 批准通过
        public async Task<ReportApproval> ApproveApprovalAsync(int reportId, string comments)
        {
            await using var db = await _contextFactory.CreateDbContextAsync();
            var report = await db.ReportApprovals.FindAsync(reportId);
            if (report != null && report.Status == ReportApprovalStatus.待批准)
            {
                report.Status = ReportApprovalStatus.批准通过;
                report.ApprovalTime = DateTime.UtcNow;
                report.ApprovalComments = comments;
                await db.SaveChangesAsync();
            }
            return report!;
        }

        // 批准驳回
        public async Task<ReportApproval> RejectApprovalAsync(int reportId, string comments)
        {
            await using var db = await _contextFactory.CreateDbContextAsync();
            var report = await db.ReportApprovals.FindAsync(reportId);
            if (report != null && report.Status == ReportApprovalStatus.待批准)
            {
                report.Status = ReportApprovalStatus.批准驳回;
                report.ApprovalTime = DateTime.UtcNow;
                report.ApprovalComments = comments;
                await db.SaveChangesAsync();
            }
            return report!;
        }

        // 删除报告
        public async Task DeleteReportAsync(int id)
        {
            await using var db = await _contextFactory.CreateDbContextAsync();
            var report = await db.ReportApprovals.FindAsync(id);
            if (report != null)
            {
                // 删除关联的文件
                if (!string.IsNullOrEmpty(report.ReportFilePath))
                {
                    await DeleteReportFileAsync(report.ReportFilePath);
                }

                db.ReportApprovals.Remove(report);
                await db.SaveChangesAsync();
            }
        }

        // 保存报告文件
        public async Task<string> SaveReportFileAsync(IBrowserFile file, string? oldFilePath = null)
        {
            try
            {
                // 删除旧文件
                if (!string.IsNullOrEmpty(oldFilePath))
                {
                    await DeleteReportFileAsync(oldFilePath);
                }

                var uploadsFolder = Path.Combine(_webHostEnvironment.WebRootPath, "reports");
                Directory.CreateDirectory(uploadsFolder);

                var fileName = $"{Guid.NewGuid()}{Path.GetExtension(file.Name)}";
                var filePath = Path.Combine(uploadsFolder, fileName);

                await using var fileStream = new FileStream(filePath, FileMode.Create, FileAccess.Write, FileShare.None);
                await using var uploadStream = file.OpenReadStream(maxAllowedSize: 50 * 1024 * 1024); // 50MB
                await uploadStream.CopyToAsync(fileStream);

                return $"/reports/{fileName}";
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error saving report file: {ex.Message}");
                throw;
            }
        }

        // 删除报告文件
        private async Task DeleteReportFileAsync(string filePath)
        {
            if (string.IsNullOrEmpty(filePath))
                return;

            await Task.Run(() =>
            {
                try
                {
                    var fullPath = Path.Combine(_webHostEnvironment.WebRootPath, filePath.TrimStart('/'));
                    if (File.Exists(fullPath))
                    {
                        File.Delete(fullPath);
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error deleting report file: {ex.Message}");
                }
            });
        }
        // 撤回报告
        public async Task<ReportApproval> WithdrawReportAsync(int reportId, string currentUserNT)
        {
            await using var db = await _contextFactory.CreateDbContextAsync();
            var report = await db.ReportApprovals.FindAsync(reportId);

            if (report == null)
            {
                throw new KeyNotFoundException("未找到该报告。");
            }

            // 只有提交人才能撤回
            if (report.SubmitterNTAccount.ToLower() != currentUserNT)
            {
                throw new UnauthorizedAccessException("您无权撤回此报告。");
            }

            // 只有在流程中（非草稿、非最终批准）的报告才能被撤回
            if (report.Status == ReportApprovalStatus.草稿 || report.Status == ReportApprovalStatus.批准通过)
            {
                throw new InvalidOperationException($"状态为 “{report.Status}” 的报告无法撤回。");
            }

            report.Status = ReportApprovalStatus.草稿;
            // 清空之前的审核/批准意见和时间
            report.ReviewComments = null;
            report.ReviewTime = null;
            report.ApprovalComments = null;
            report.ApprovalTime = null;

            await db.SaveChangesAsync();
            return report;
        }
        public async Task DeleteReportAsync(int reportId, string currentUserNT)
        {
            await using var db = await _contextFactory.CreateDbContextAsync();
            var report = await db.ReportApprovals.FindAsync(reportId);

            if (report == null)
            {
                throw new KeyNotFoundException("未找到该报告。");
            }

            // 只有提交人可以删除
            if (report.SubmitterNTAccount != currentUserNT)
            {
                throw new UnauthorizedAccessException("您无权删除此报告。");
            }

            // 只有草稿可以被删除
            if (report.Status != ReportApprovalStatus.草稿)
            {
                throw new InvalidOperationException("只有“草稿”状态的报告可以被删除。");
            }

            // 删除关联的文件
            if (!string.IsNullOrEmpty(report.ReportFilePath))
            {
                await DeleteReportFileAsync(report.ReportFilePath);
            }

            db.ReportApprovals.Remove(report);
            await db.SaveChangesAsync();
        }

        public async Task<string> GetNextReportNumberAsync()
        {
            await using var db = await _contextFactory.CreateDbContextAsync();

            var today = DateTime.UtcNow;
            var datePrefix = $"C{today:yyMMdd}"; 

            // 查找今天已经创建的所有报告
            var todaysReports = await db.ReportApprovals
                .Where(r => r.ReportNumber.StartsWith(datePrefix))
                .Select(r => r.ReportNumber)
                .ToListAsync();

            int maxSequence = 0;
            if (todaysReports.Any())
            {
                // 从报告编号中解析出序列号，并找到最大值
                maxSequence = todaysReports
                    .Select(num => {
                        if (num.StartsWith("C") && num.Length >= 9)
                        {
                            // 取最后2位作为序列号
                            string seqStr = num.Substring(num.Length - 2);
                            if (int.TryParse(seqStr, out int seq))
                            {
                                return seq;
                            }
                        }
                        return 0; // 如果格式不匹配，则忽略
                    })
                    .DefaultIfEmpty(0) // 如果没有有效序列号，则返回0
                    .Max();
            }

            // 新的序列号是最大值 + 1
            int nextSequence = maxSequence + 1;

            // 格式化为三位数的字符串，例如 1 -> "001"
            return $"{datePrefix}{nextSequence:D2}";
        }

        public async Task<bool> IsReportNumberUniqueAsync(string reportNumber, int currentReportId)
        {
            await using var db = await _contextFactory.CreateDbContextAsync();

            // 检查数据库中是否存在其他报告使用了这个编号。
            // 忽略大小写，并排除当前正在编辑的报告本身。
            return !await db.ReportApprovals
                .AnyAsync(r => r.ReportNumber.ToLower() == reportNumber.ToLower() && r.Id != currentReportId);
        }

    }
}