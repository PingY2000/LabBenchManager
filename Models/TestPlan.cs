// Models/TestPlan.cs
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

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

    public enum PriorityLevel
    {
        低 = 1,
        中 = 2,
        高 = 3,
        紧急 = 4
    }

    public class TestPlan
    {
        public int Id { get; set; }

        // === 关联到测试台 ===
        [Required]
        [Display(Name = "测试台")]
        public int BenchId { get; set; }

        [ForeignKey("BenchId")]
        public virtual Bench? Bench { get; set; }

        // 新增：可选的外键关联到 Assignment
        [Display(Name = "关联的测试申请")]
        public int? AssignmentId { get; set; }

        [ForeignKey("AssignmentId")]
        public virtual Assignment? Assignment { get; set; }

        // === 测试计划基本信息 ===
        [Required(ErrorMessage = "测试项目名称不能为空")]
        [StringLength(200)]
        [Display(Name = "测试项目名称")]
        public string ProjectName { get; set; } = string.Empty;

        [StringLength(500)]
        [Display(Name = "测试内容描述")]
        public string? Description { get; set; }

        // === 排程信息 ===
        [Display(Name = "优先级")]
        public PriorityLevel Priority { get; set; } = PriorityLevel.中;

        [Display(Name = "排序序号")]
        public int SortOrder { get; set; } // 用于手动调整队列顺序

        [Display(Name = "状态")]
        public TestPlanStatus Status { get; set; } = TestPlanStatus.待开始;

        // === 时间信息 ===
        [Display(Name = "计划开始时间")]
        public DateTime? PlannedStartTime { get; set; }

        [Display(Name = "计划结束时间")]
        public DateTime? PlannedEndTime { get; set; }

        [Display(Name = "实际开始时间")]
        public DateTime? ActualStartTime { get; set; }

        [Display(Name = "实际结束时间")]
        public DateTime? ActualEndTime { get; set; }

        // === 人员信息 ===
        [StringLength(100)]
        [Display(Name = "测试负责人")]
        public string? AssignedTo { get; set; } // NT账号或姓名

        [StringLength(100)]
        [Display(Name = "申请人")]
        public string? RequestedBy { get; set; }

        // === 其他信息 ===
        [StringLength(50)]
        [Display(Name = "样品编号")]
        public string? SampleNumber { get; set; }

        [Display(Name = "样品数量")]
        public int? SampleQuantity { get; set; }

        [StringLength(1000)]
        [Display(Name = "备注")]
        public string? Notes { get; set; }

        [Display(Name = "创建时间")]
        public DateTime CreatedAt { get; set; } = DateTime.Now;

        [Display(Name = "最后更新时间")]
        public DateTime? UpdatedAt { get; set; }
    }
}