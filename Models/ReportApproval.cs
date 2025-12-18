// Models/ReportApproval.cs
using System.ComponentModel.DataAnnotations;

namespace LabBenchManager.Models
{
    public enum ReportApprovalStatus
    {
        草稿,
        待审核,
        审核通过,
        审核驳回,
        待批准,
        批准通过,
        批准驳回
    }

    public class ReportApproval
    {
        public int Id { get; set; }

        [Required]
        [StringLength(200)]
        public string ReportTitle { get; set; } = string.Empty;

        [Required]
        [StringLength(100)]
        public string ReportNumber { get; set; } = string.Empty;

        // 关联的申请
        public int? AssignmentId { get; set; }
        public Assignment? Assignment { get; set; }

        // 报告文件路径
        [StringLength(500)]
        public string? ReportFilePath { get; set; }

        // 提交人信息
        [Required]
        [StringLength(100)]
        public string SubmitterNTAccount { get; set; } = string.Empty;

        [StringLength(100)]
        public string? SubmitterName { get; set; }

        public DateTime SubmitTime { get; set; } = DateTime.UtcNow;

        // 审核人信息
        [Required]
        [StringLength(100)]
        public string ReviewerNTAccount { get; set; } = string.Empty;

        [StringLength(100)]
        public string? ReviewerName { get; set; }

        public DateTime? ReviewTime { get; set; }

        [StringLength(500)]
        public string? ReviewComments { get; set; }

        // 批准人信息
        [Required]
        [StringLength(100)]
        public string ApproverNTAccount { get; set; } = string.Empty;

        [StringLength(100)]
        public string? ApproverName { get; set; }

        public DateTime? ApprovalTime { get; set; }

        [StringLength(500)]
        public string? ApprovalComments { get; set; }

        // 状态
        public ReportApprovalStatus Status { get; set; } = ReportApprovalStatus.草稿;

        // 报告摘要
        [StringLength(1000)]
        public string? Summary { get; set; }

        // 备注
        [StringLength(1000)]
        public string? Notes { get; set; }
    }
}