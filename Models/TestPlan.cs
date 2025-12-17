// Models/TestPlan.cs

using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Globalization;

namespace LabBenchManager.Models
{
    public enum TestPlanStatus
    {
        待开始,     // Pending
        进行中,     // In Progress
        已完成,     // Completed
        已暂停,     // On Hold
        已取消      // Cancelled
    }

    // PriorityLevel 枚举已不再需要，可以删除

    public class TestPlan
    {
        public int Id { get; set; }

        [Required]
        public int BenchId { get; set; }

        [ForeignKey("BenchId")]
        public virtual Bench? Bench { get; set; }

        public int? AssignmentId { get; set; }

        [ForeignKey("AssignmentId")]
        public virtual Assignment? Assignment { get; set; }

        [Required(ErrorMessage = "测试项目名称不能为空")]
        [StringLength(200)]
        public string ProjectName { get; set; } = string.Empty;

        [StringLength(500)]
        public string? Description { get; set; }

        public TestPlanStatus Status { get; set; } = TestPlanStatus.待开始;

        // 新增：用于存储以逗号分隔的日期字符串，例如 "2023-10-26,2023-10-28"
        public string ScheduledDates { get; set; } = string.Empty;

        [StringLength(100)]
        public string? AssignedTo { get; set; }

        // 其他字段保持不变...
        public string? RequestedBy { get; set; }
        public string? SampleNumber { get; set; }
        public int? SampleQuantity { get; set; }
        public string? Notes { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.Now;
        public DateTime? UpdatedAt { get; set; }

        // === 辅助方法 (不在数据库中映射) ===

        [NotMapped]
        public List<DateTime> ScheduledDateList => GetScheduledDates();

        /// <summary>
        /// 将存储的日期字符串解析为 DateTime 列表
        /// </summary>
        public List<DateTime> GetScheduledDates()
        {
            if (string.IsNullOrWhiteSpace(ScheduledDates))
            {
                return new List<DateTime>();
            }
            return ScheduledDates.Split(',')
                                 .Select(s => DateTime.ParseExact(s, "yyyy-MM-dd", CultureInfo.InvariantCulture))
                                 .ToList();
        }

        /// <summary>
        /// 设置要存储的日期列表，并将其转换为字符串
        /// </summary>
        public void SetScheduledDates(List<DateTime> dates)
        {
            if (dates == null || !dates.Any())
            {
                ScheduledDates = string.Empty;
                return;
            }
            ScheduledDates = string.Join(",", dates.OrderBy(d => d).Select(d => d.ToString("yyyy-MM-dd")));
        }
    }
}