using System.ComponentModel.DataAnnotations;

namespace LabBenchManager.Models
{
    public class TestPlanHistory
    {
        public int Id { get; set; }

        [Required]
        public int TestPlanId { get; set; }

        [Required]
        public DateTime ModifiedAt { get; set; } = DateTime.Now;

        [Required]
        public string ModifiedBy { get; set; } = "";

        [Required]
        public string ChangeDescription { get; set; } = "";

        // 修改前的状态快照（JSON格式）
        public string? PreviousSnapshot { get; set; }

        // 修改后的状态快照（JSON格式）
        public string? NewSnapshot { get; set; }

        // 具体修改的字段
        public string? ChangedFields { get; set; }

        // 修改原因
        public string? Reason { get; set; }

        // 导航属性
        public virtual TestPlan? TestPlan { get; set; }
    }
}