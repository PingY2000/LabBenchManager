// Models/Bench.cs
using System.ComponentModel.DataAnnotations;

namespace LabBenchManager.Models
{
    public enum BenchStatus
    {
        空闲,
        使用中,
        维护中,
        已预定
    }
    public class Bench
    {
        // --- Primary Key ---
        public int Id { get; set; }

        // --- Static Properties (from image, defining the bench) ---

        [Required(ErrorMessage = "设备名称不能为空")]
        [StringLength(100)]
        [Display(Name = "设备名称")]
        public string Name { get; set; } = string.Empty; // e.g., "VO36_General function hydraulic test bench"

        [StringLength(50)]
        [Display(Name = "设备编号")]
        public string? EquipmentNo { get; set; } // e.g., "12235422"

        [StringLength(50)]
        [Display(Name = "资产编号")]
        public string? AssetNo { get; set; } // e.g., "105500"

        [StringLength(100)]
        [Display(Name = "位置")]
        public string? Location { get; set; } // e.g., "DCCC-204-1F,Lab DC-IH/ECH-CN"

        [StringLength(50)]
        [Display(Name = "测试类型")]
        public string? TestType { get; set; } // e.g., "Functional test"

        [StringLength(50)]
        [Display(Name = "测试对象")]
        public string? TestObject { get; set; } // e.g., "valve"

        [Display(Name = "数量")]
        public int Quantity { get; set; } = 1;

        [StringLength(50)]
        [Display(Name = "工时规范")]
        public string? WorkingHoursNorm { get; set; } // e.g., "1 x 8 h x 5 day"

        [StringLength(500)]
        [Display(Name = "基本性能与配置")]
        public string? BasicPerformanceAndConfiguration { get; set; } // e.g., "3 working areas A1 A2 A3"

        [StringLength(255)]
        [Display(Name = "图片")]
        public string? PictureUrl { get; set; } // URL or path to the picture

        // --- Dynamic Properties (describing current state) ---
        // 注意：状态字段已移除，状态应该从 TestPlan 中计算得出

        [StringLength(50)]
        [Display(Name = "当前使用者")]
        public string? CurrentUser { get; set; }

        [StringLength(100)]
        [Display(Name = "当前项目")]
        public string? Project { get; set; }

        [Display(Name = "下次可用时间")]
        public DateTime? NextAvailableTime { get; set; }

        // --- Navigation Properties ---
        // 如果需要关联测试计划
        public ICollection<TestPlan>? TestPlans { get; set; }
    }
}