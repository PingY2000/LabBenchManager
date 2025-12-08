// Models/Assignment.cs
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace LabBenchManager.Models
{
    // 状态枚举
    public enum AssignmentStatus { 待审批, 已批准, 已拒绝, 进行中, 已完成 }

    public class Assignment
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "申请人姓名不能为空")]
        [StringLength(50)]
        public string ApplicantName { get; set; } = "";

        [Required(ErrorMessage = "项目名称不能为空")]
        [StringLength(100)]
        public string ProjectName { get; set; } = "";

        // --- 外键关联到 Bench 模型 ---
        [Range(1, int.MaxValue, ErrorMessage = "请选择一个有效的测试设备")]
        public int BenchId { get; set; } // 这是外键字段

        [ForeignKey("BenchId")]
        public virtual Bench? Bench { get; set; } // 这是导航属性
        // --------------------------

        public DateTime RequestTime { get; set; }

        [Required(ErrorMessage = "必须指定计划开始时间")]
        public DateTime StartTime { get; set; } = DateTime.Today;

        [Required(ErrorMessage = "必须指定计划结束时间")]
        public DateTime EndTime { get; set; } = DateTime.Today.AddDays(1);

        public AssignmentStatus Status { get; set; }

        [StringLength(500)]
        public string? Notes { get; set; }
    }
}